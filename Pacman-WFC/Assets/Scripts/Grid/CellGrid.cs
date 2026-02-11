using JFlex.PacmanWFC.View;
using System.Collections.Generic;

namespace JFlex.PacmanWFC.Data
{
    public enum UPDATE_TYPE
    {
        Cell,
        Cells,
        Reset,
        Complete
    }

    public class GenerationProgress
    {
        public UPDATE_TYPE Type;
        public CellObj UpdatedCell;
        public List<CellObj> UpdatedCells;
        public float Delay;

        public GenerationProgress Reset(float delay)
        {
            Type = UPDATE_TYPE.Reset;
            Delay = delay;

            return this;
        }

        public GenerationProgress Set(UPDATE_TYPE type, float delay)
        {
            Type = type;
            Delay =delay;

            return this;
        }

        public GenerationProgress Set(CellObj updatedCell, float delay)
        {
            Type = UPDATE_TYPE.Cell;
            UpdatedCell = updatedCell;
            Delay = delay;

            return this;
        }

        public GenerationProgress Set(List<CellObj> updatedCells, float delay)
        {
            Type = UPDATE_TYPE.Cells;
            UpdatedCells = updatedCells;
            Delay = delay;

            return this;
        }
    }

    public class CellGrid<T> : ICellGrid<T> where T : CellObj
    {
        private enum WAVEFUNCTIONSTATUS
        {
            NOTSTARTED,
            RUNNING,
            FINISHED,
            ERROR,
            INVALID
        }

        private readonly T[,] cellArray;
        private readonly int height;
        private readonly int width;

        private readonly int genWidth;
        private readonly int middle;

        public int WidthToGenerate => genWidth;

        private bool isComplete;
        public bool IsComplete => isComplete;

        T graphSearchStartCell;

        private TilesConfig tilesConfig;

        private readonly int minimumNonEmptyCount;

        public int NonEmptyTileCount { get; private set; }

        private readonly EdgeSpritesMapping edgeSpritesMapping;

        private UIDelegates.OnStatusUpdateCallback statusUpdateCallback;

        private GenerationProgress progress;
        private DelayTimings delayTimings;

        private PacmanGraph pacmanGraph;

        public CellGrid(int height, int width, TilesConfig tilesConfig, EdgeSpritesMapping edgeSpritesMapping, UIDelegates.OnStatusUpdateCallback onStatusUpdate)
        {
            this.tilesConfig = tilesConfig;
            this.edgeSpritesMapping = edgeSpritesMapping;
            statusUpdateCallback = onStatusUpdate;

            this.height = height;
            this.width = width;

            genWidth = this.width / 2;
            middle = this.height / 2;

            cellArray = new T[height, width];

            // Half of the generated grid should be non-empty to be considered valid.
            minimumNonEmptyCount = (height * genWidth) / 3;
            NonEmptyTileCount = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    T cell;
                    if (x < genWidth)
                    {
                        var directions = Direction.None;

                        if (y > 0)
                        {
                            directions |= Direction.Down;
                        }
                        if (y < height - 1)
                        {
                            directions |= Direction.Up;
                        }
                        if (x > 0)
                        {
                            directions |= Direction.Left;
                        }
                        if (x < width - 1)
                        {
                            directions |= Direction.Right;
                        }

                        cell = new CellObj(x, y, tilesConfig.Tiles, directions) as T;
                    }
                    else
                    {
                        cell = new CellObj(x, y) as T;
                    }

                    cellArray[y, x] = cell;
                }
            }
        }

        public IEnumerator<GenerationProgress> GenerateGrid(DelayTimings delayTimings)
        {
            progress = new GenerationProgress();
            this.delayTimings = delayTimings;
            isComplete = false;

            for (var genAttempts = 0; genAttempts < 20; genAttempts++)
            {
                statusUpdateCallback("Placing tunnel and ghost box");

                var tunnelUpdatedCells = PlaceTunnels();
                yield return progress.Set(tunnelUpdatedCells, delayTimings.GeneratingDelay);

                var ghostBoxUpdatedCells = PlaceGhostBoxCells();
                yield return progress.Set(ghostBoxUpdatedCells, delayTimings.GeneratingDelay);

                // Create Pacman Graph for checking connectivity
                pacmanGraph = new PacmanGraph(height, genWidth, graphSearchStartCell);

                // wave function collapse
                var waveFunctionStatus = WAVEFUNCTIONSTATUS.NOTSTARTED;
                var attempts = 0;

                while (waveFunctionStatus != WAVEFUNCTIONSTATUS.FINISHED && attempts < 20)
                {
                    statusUpdateCallback("RUNNING WAVEFORM COLLAPSE GENERATION");

                    List<CellObj> updatedCells = new();

                    waveFunctionStatus = WaveFunctionCollapseStep(updatedCells);

                    if (waveFunctionStatus == WAVEFUNCTIONSTATUS.ERROR)
                    {
                        yield return progress.Reset(delayTimings.ResetDelay);

                        attempts++;
                        continue;
                    }

                    if (waveFunctionStatus == WAVEFUNCTIONSTATUS.INVALID)
                    {
                        statusUpdateCallback("Can no longer reach all cells");

                        // return the failing cell so it can be shown before resetting!
                        yield return progress.Set(updatedCells, delayTimings.ResetDelay);

                        yield return progress.Reset(delayTimings.ResetDelay);

                        attempts++;
                        continue;
                    }

                    yield return progress.Set(updatedCells, delayTimings.GeneratingDelay);
                }

                statusUpdateCallback("Checking grid is valid");

                if (IsValidGrid())
                {
                    break;
                }

                statusUpdateCallback("Restarting");
                ResetCells();

                yield return progress.Reset(delayTimings.ResetDelay);
            }

            var completeGridRoutine = CompleteGridStep();

            while (completeGridRoutine.MoveNext())
            {
                yield return completeGridRoutine.Current;
            }

            isComplete = true;
            yield return progress.Set(UPDATE_TYPE.Complete, delayTimings.GeneratingDelay);
        }

        private WAVEFUNCTIONSTATUS WaveFunctionCollapseStep(List<CellObj> updatedCells)
        {
            if (!TryGetCellWithLowestEntropy(out var cellToCollapse))
            {
                return WAVEFUNCTIONSTATUS.FINISHED;
            }

            if (!TryCollapseCell(cellToCollapse, out var triggerConnectivityCheck))
            {
                return WAVEFUNCTIONSTATUS.ERROR;
            }

            updatedCells.Add(cellToCollapse);

            if (triggerConnectivityCheck)
            {
                if (!pacmanGraph.IsStillConnected(cellArray))
                {
                    return WAVEFUNCTIONSTATUS.INVALID;
                }
            }
            
            var cellStack = new Stack<CellObj>();
            cellStack.Push(cellToCollapse);

            while (cellStack.Count > 0)
            {
                var cell = cellStack.Pop();
                foreach (Direction dir in DirectionExtensions.AllDirections)
                {
                    if (TryConstrainNeighbourOfCell(cell, dir, out var neighbour))
                    {
                        cellStack.Push(neighbour);

                        updatedCells.Add(neighbour);
                    }
                }
            }

            return WAVEFUNCTIONSTATUS.RUNNING;
        }

        public bool TryCollapseCell(T cellToCollapse, out bool triggerConnectivityCheck)
        {
            triggerConnectivityCheck = false;

            if (cellToCollapse.TryCollapse(out var lostConnections))
            {
                if (!cellToCollapse.CollapsedTile.IsEmptyTile)
                {
                    NonEmptyTileCount++;
                }

                if (lostConnections != Direction.None)
                {
                    triggerConnectivityCheck = true;
                }

                return true;
            }

            return false;
        }

        public List<CellObj> PlaceGhostBoxCells()
        {
            var updatedCells = new List<CellObj>();

            var cell = cellArray[middle, genWidth - 1];
            cell.SetTile(tilesConfig.EmptyTile, doNotMirror: true);
            updatedCells.Add(cell);

            // Then restrict the tiles above, below and to the left of the box
            if (TryGetCellNeighbour(cell, Direction.Up, out graphSearchStartCell))
            {
                graphSearchStartCell.ResetCell(tilesConfig.AboveBoxTiles);
                updatedCells.Add(graphSearchStartCell);
            }

            if (TryGetCellNeighbour(cell, Direction.Down, out var belowBoxCell))
            {
                belowBoxCell.ResetCell(tilesConfig.BelowBoxTiles);
                updatedCells.Add(belowBoxCell);
            }

            if (TryGetCellNeighbour(cell, Direction.Left, out var leftBoxCell))
            {
                leftBoxCell.ResetCell(tilesConfig.SideBoxTiles);
                updatedCells.Add(leftBoxCell);
            }

            return updatedCells;
        }

        public List<CellObj> PlaceTunnels()
        {
            var updatedCells = new List<CellObj>();

            var firstCell = cellArray[middle, 0];
            var secondCell = cellArray[middle, 1];

            firstCell.SetTile(tilesConfig.TunnelTile, doNotMirror: true);
            updatedCells.Add(firstCell);

            secondCell.SetTile(tilesConfig.TunnelTile, doNotMirror: true);
            updatedCells.Add(secondCell);

            // Manually update the non-empty tile count and the gridGraph.
            NonEmptyTileCount += 2;

            var aboveRow = middle;
            var belowRow = middle;
            T neighbourCell;

            if (height >= 7)
            {
                // If the grid is tall enough, there is enough space to leave empty space around
                // the tunnel
                for (var x = 0; x <= 1; x++)
                {
                    var cell = cellArray[middle - 1, x];
                    cell.SetTile(tilesConfig.EmptyTile, doNotMirror: true);
                    updatedCells.Add(cell);

                    cell = cellArray[middle + 1, x];
                    cell.SetTile(tilesConfig.EmptyTile, doNotMirror: true);
                    updatedCells.Add(cell);
                }

                // Adjust the values of the rows above and below that now need constraining
                aboveRow++;
                belowRow--;

                // The tiles to the right of the empty row cells now also need constraining.
                if (TryConstrainNeighbourOfCell(cellArray[aboveRow, 1], Direction.Right, out neighbourCell))
                {
                    updatedCells.Add(neighbourCell);
                }

                if (TryConstrainNeighbourOfCell(cellArray[belowRow, 1], Direction.Right, out neighbourCell))
                {
                    updatedCells.Add(neighbourCell);
                }
            }

            for (var x = 0; x <= 1; x++)
            {
                if (TryConstrainNeighbourOfCell(cellArray[aboveRow, x], Direction.Up, out neighbourCell))
                {
                    updatedCells.Add(neighbourCell);
                }

                if (TryConstrainNeighbourOfCell(cellArray[belowRow, x], Direction.Down, out neighbourCell))
                {
                    updatedCells.Add(neighbourCell);
                }
            }

            if (TryConstrainNeighbourOfCell(cellArray[middle, 1], Direction.Right, out neighbourCell))
            {
                updatedCells.Add(neighbourCell);
            }

            return updatedCells;
        }

        public bool CellAlreadyPlaced(int x, int y)
        {
            return (cellArray[y, x] != null);
        }

        public T GetCell(int x, int y)
        {
            return (T)cellArray[y, x];
        }

        public void ResetCells()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < genWidth; x++)
                {
                    if (!cellArray[y, x].DoNotMirror)
                    {
                        cellArray[y, x].ResetCell(tilesConfig.Tiles);
                    }
                }
            }

            NonEmptyTileCount = 0;
        }

        public bool TryGetCellNeighbour(T cell, Direction direction, out T neighbour)
        {
            neighbour = null;
            switch (direction)
            {
                case Direction.Up:

                    if (cell.Y == height - 1)
                    {
                        return false;
                    }

                    neighbour = (T)cellArray[cell.Y + 1, cell.X];
                    return true;

                case Direction.Down:

                    if (cell.Y == 0)
                    {
                        return false;
                    }

                    neighbour = (T)cellArray[cell.Y - 1, cell.X];
                    return true;

                case Direction.Left:

                    if (cell.X == 0)
                    {
                        return false;
                    }

                    neighbour = (T)cellArray[cell.Y, cell.X - 1];
                    return true;

                case Direction.Right:

                    if (cell.X == genWidth - 1)
                    {
                        return false;
                    }

                    neighbour = (T)cellArray[cell.Y, cell.X + 1];
                    return true;
            }

            return false;
        }

        public bool TryGetCellWithLowestEntropy(out T cell)
        {
            var cells = GetLowestEntropy();

            if (cells.Count == 0)
            {
                cell = null;
                return false;
            }

            var idx = UnityEngine.Random.Range(0, cells.Count);
            cell = cells[idx] as T;

            return true;
        }

        public List<CellObj> GetLowestEntropy()
        {
            var lowestEntropyCells = new List<CellObj>();
            var lowestEntropy = tilesConfig.Tiles.Count;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < genWidth; x++)
                {
                    var cell = GetCell(x, y);
                    if (cell.IsCollapsed)
                    {
                        continue;
                    }

                    if (cell.TileCount < lowestEntropy)
                    {
                        lowestEntropyCells.Clear();
                        lowestEntropyCells.Add(cell);
                        lowestEntropy = cell.TileCount;
                    }
                    else if (cell.TileCount == lowestEntropy)
                    {
                        lowestEntropyCells.Add(cell);
                    }
                }
            }

            return lowestEntropyCells;
        }

        public bool TryConstrainNeighbourOfCell(CellObj cell, Direction dir, out T neighbour)
        {
            var neighbourConstrained = false;
            if (TryGetCellNeighbour((T)cell, dir, out neighbour) && !neighbour.IsCollapsed)
            {
                if (neighbour.TryConstrain(cell.Tiles, dir))
                {
                    neighbourConstrained = true;
                }
            }

            return neighbourConstrained;
        }

        public bool IsValidGrid()
        {
            return pacmanGraph.IsStillConnected(cellArray);
        }

        public IEnumerator<GenerationProgress> CompleteGridStep()
        {
            statusUpdateCallback("Completing Grid");

            var updatePerColumn = (height > 20);
            
            for (int x = genWidth - 1, z = genWidth; x >= 0; x--, z++)
            {
                List<CellObj> updatedCells = new();

                for (var y = 0; y < height; y++)
                {
                    var cellToMirror = GetCell(x, y);
                    var mirroredTile = tilesConfig.GetMirroredTile(cellToMirror.Tiles[0]);

                    cellArray[y, z].SetTile(mirroredTile);
                    updatedCells.Add(cellArray[y, z]);

                    if (!updatePerColumn)
                    {
                        yield return progress.Set(cellArray[y, z], delayTimings.CompletingDelay);
                    }                    
                }

                if (updatePerColumn)
                {
                    yield return progress.Set(updatedCells, delayTimings.CompletingDelay);
                }
            }

            var addOutsideEdgesRoutine = AddOutsideEdgesToGridStep();

            while (addOutsideEdgesRoutine.MoveNext())
            {
                yield return addOutsideEdgesRoutine.Current;
            }

            var ghostBoxCells = AddGhostBoxEdges();
            yield return progress.Set(ghostBoxCells, delayTimings.GeneratingDelay);
        }

        public IEnumerator<GenerationProgress> AddOutsideEdgesToGridStep()
        {
            statusUpdateCallback("Adding outside edges and ghost box to grid");

            T start = default;
            Direction dir = Direction.Right;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (IsBoundaryCell(x, y))
                    {
                        dir = Direction.Right;
                        start = cellArray[y, x];

                        foreach (var d in DirectionExtensions.AllDirections)
                        {
                            var (dx, dy) = DirectionExtensions.LookInDirection(d);
                            var newX = x + dx;
                            var newY = y + dy;

                            if (!TryGetCell(newX, newY, out var testCell) || !IsInsideRegion(testCell))
                            {
                                dir = DirectionExtensions.TurnLeft(d);
                                break;
                            }
                        }

                        break;
                    }
                }

                if (start != default(T))
                {
                    // break outer loop!
                    break;
                }
            }

            var current = start;

            do
            {
                // look in all directions to see what edges are on the outside
                // Once we have these, update the cell with double edges.
                var outsideEdges = GetOutsideEdges(current.X, current.Y);

                current.CollapsedTile.AddDoubleEdges(outsideEdges, edgeSpritesMapping);

                yield return progress.Set(current, delayTimings.CompletingDelay);

                Direction[] checkOrder =
                {
                    DirectionExtensions.TurnRight(dir),
                    dir,
                    DirectionExtensions.TurnLeft(dir)
                };

                foreach (var d in checkOrder)
                {
                    var (dx, dy) = DirectionExtensions.LookInDirection(d);
                    var newX = current.X + dx;
                    var newY = current.Y + dy;

                    if (TryGetCell(newX, newY, out var newCell) && IsInsideRegion(newCell))
                    {
                        dir = d;
                        current = newCell;
                        break;
                    }
                }

            } while (current != start);
        }

        private bool TryAddDoubleEdgeToCell(int x, int y, SPRITEPART_SECTION spritePartSection, out T cell)
        {
            if (0 > y || y > width || 0 > x || x > height)
            {
                cell = null;
                return false;
            }

            cell = cellArray[y, x];
            cell.CollapsedTile.AddDoubleEdge(spritePartSection);

            return true;
        }

        private List<CellObj> AddGhostBoxEdges()
        {
            var updatedCells = new List<CellObj>();

            if (TryAddDoubleEdgeToCell(genWidth - 1, middle + 1, SPRITEPART_SECTION.GHOST_LEFT_ENTRANCE, out T cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth, middle + 1, SPRITEPART_SECTION.GHOST_RIGHT_ENTRANCE, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth - 2, middle + 1, SPRITEPART_SECTION.GHOST_TOP_LEFT, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth + 1, middle + 1, SPRITEPART_SECTION.GHOST_TOP_RIGHT, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth - 2, middle, SPRITEPART_SECTION.RIGHT_SIDE, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth - 2, middle - 1, SPRITEPART_SECTION.GHOST_BOTTOM_LEFT, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth + 1, middle - 1, SPRITEPART_SECTION.GHOST_BOTTOM_RIGHT, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth + 1, middle, SPRITEPART_SECTION.LEFT_SIDE, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth - 1, middle - 1, SPRITEPART_SECTION.TOP_SIDE, out cell))
            {
                updatedCells.Add(cell);
            }

            if (TryAddDoubleEdgeToCell(genWidth, middle - 1, SPRITEPART_SECTION.TOP_SIDE, out cell))
            {
                updatedCells.Add(cell);
            }

            return updatedCells;
        }

        private bool IsBoundaryCell(int x, int y)
        {
            if (!TryGetCell(x, y, out var cell) || !IsInsideRegion(cell))
                return false;

            // Check neighbors for touching "outside"
            foreach (Direction d in DirectionExtensions.AllDirections)
            {
                var (dx, dy) = DirectionExtensions.LookInDirection(d);
                int nx = x + dx;
                int ny = y + dy;

                if (!TryGetCell(nx, ny, out var neighbor) || !IsInsideRegion(neighbor))
                    return true; // touches outside
            }

            return false;
        }

        private Direction GetOutsideEdges(int x, int y)
        {
            Direction outside = Direction.None;

            foreach (var d in DirectionExtensions.AllDirections)
            {
                var (dx, dy) = DirectionExtensions.LookInDirection(d);
                var newX = x + dx;
                var newY = y + dy;

                if (!TryGetCell(newX, newY, out var testCell) || !IsInsideRegion(testCell))
                {
                    outside |= d;
                }
            }

            return outside;
        }

        private bool IsInsideRegion(T cell)
        {
            return !cell.CollapsedTile.Equals(tilesConfig.EmptyTile)
               && !cell.CollapsedTile.Equals(tilesConfig.TunnelTile);
        }

        private bool TryGetCell(int x, int y, out T cell)
        {
            cell = null;

            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                cell = cellArray[y, x];
                return true;
            }

            return false;
        }
    }
}