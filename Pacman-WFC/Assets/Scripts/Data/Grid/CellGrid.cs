using JFlex.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    public class CellGrid<T> : ICellGrid<T> where T : CellObj
    {
        private T[,] cellArray;
        private int height;
        private int width;

        private int genWidth;
        private int middle;

        public int WidthToGenerate => genWidth;

        private bool isComplete;
        public bool IsComplete => isComplete;

        private Graph<T> gridGraph;

        T graphSearchStartTile;

        private TilesConfig tilesConfig;

        private int minimumNonEmptyCount;

        public int NonEmptyTileCount { get; private set; }

        private readonly EdgeSpritesMapping edgeSpritesMapping;

        private Action<string> onStatusUpdate;


        public CellGrid(int height, int width, TilesConfig tilesConfig, EdgeSpritesMapping edgeSpritesMapping, Action<string> onStatusUpdate)
        {
            this.tilesConfig = tilesConfig;
            this.edgeSpritesMapping = edgeSpritesMapping;
            this.onStatusUpdate = onStatusUpdate;

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

                        var isOpenCell = (x == width - 1);

                        if (isOpenCell)
                        {
                            Debug.Log($"Is Open Cell {x},{y}");
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

            gridGraph = new Graph<T>();
        }

        public bool TryCollapseCell(T cellToCollapse)
        {
            bool collapsed = cellToCollapse.TryCollapse();

            if (collapsed)
            {
                if (!cellToCollapse.CollapsedTile.IsEmptyTile)
                {
                    NonEmptyTileCount++;
                }

                // add connected neighbours to graph...
                foreach (Direction dir in DirectionExtensions.AllDirections)
                {
                    if (dir == Direction.None)
                    {
                        continue;
                    }

                    if (TryGetCellNeighbour(cellToCollapse, dir, out var neighbour) && neighbour.IsCollapsed)
                    {
                        if (cellToCollapse.Tiles[0].SharedEdgeWithNeighbour(neighbour.Tiles[0], dir))
                        {
                            gridGraph.AddEdge(cellToCollapse, neighbour);
                            gridGraph.AddEdge(neighbour, cellToCollapse);
                        }
                    }
                }
            }

            return collapsed;
        }

        public void PlaceGhostSpawnCells()
        {
            // Set the ghost spawn cell (using empty sprites)
            var cell = cellArray[middle, genWidth - 1];
            cell.SetTile(tilesConfig.EmptyTile, doNotMirror: true);

            // Then restrict the tiles above, below and to the left of the box
            if (TryGetCellNeighbour(cell, Direction.Up, out graphSearchStartTile))
            {
                graphSearchStartTile.ResetCell(tilesConfig.AboveBoxTiles);
            }

            if (TryGetCellNeighbour(cell, Direction.Down, out var belowBoxTile))
            {
                belowBoxTile.ResetCell(tilesConfig.BelowBoxTiles);
            }

            if (TryGetCellNeighbour(cell, Direction.Left, out var leftBoxTile))
            {
                leftBoxTile.ResetCell(tilesConfig.SideBoxTiles);
            }
        }

        public void PlaceTunnels()
        {
            var firstCell = cellArray[middle, 0];
            var secondCell = cellArray[middle, 1];

            firstCell.SetTile(tilesConfig.TunnelTile, doNotMirror: true);
            secondCell.SetTile(tilesConfig.TunnelTile, doNotMirror: true);

            // Manually update the non-empty tile count and the gridGraph.
            NonEmptyTileCount += 2;
            gridGraph.AddEdge(firstCell, secondCell);
            gridGraph.AddEdge(secondCell, firstCell);

            var aboveRow = middle;
            var belowRow = middle;

            if (height >= 7)
            {
                // If the grid is tall enough, there is enough space to leave empty space around
                // the tunnel
                for (var x = 0; x <= 1; x++)
                {
                    cellArray[middle - 1, x].SetTile(tilesConfig.EmptyTile, doNotMirror: true);
                    cellArray[middle + 1, x].SetTile(tilesConfig.EmptyTile, doNotMirror: true);
                }

                // Adjust the values of the rows above and below that now need constraining
                aboveRow++;
                belowRow--;

                // The tiles to the right of the empty row cells now also need constraining.
                TryConstrainNeighbourOfCell(cellArray[aboveRow, 1], Direction.Right, out _);
                TryConstrainNeighbourOfCell(cellArray[belowRow, 1], Direction.Right, out _);
            }

            for (var x = 0; x <= 1; x++)
            {
                TryConstrainNeighbourOfCell(cellArray[aboveRow, x], Direction.Up, out _);
                TryConstrainNeighbourOfCell(cellArray[belowRow, x], Direction.Down, out _);
            }

            TryConstrainNeighbourOfCell(cellArray[middle, 1], Direction.Right, out _);
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

            NonEmptyTileCount = 1;
            gridGraph = new Graph<T>();
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

        public bool TryGetCellWithLowestEntropy(out CellObj cell)
        {
            var cells = GetLowestEntropy();

            if (cells.Count == 0)
            {
                cell = null;
                return false;
            }

            var idx = UnityEngine.Random.Range(0, cells.Count);
            cell = cells[idx];

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
            neighbour = null;
            if (((dir & cell.ValidDirections) != 0 || cell.ValidDirections == Direction.None) &&
                TryGetCellNeighbour((T)cell, dir, out neighbour))
            {
                if (!neighbour.IsCollapsed)
                {
                    if (neighbour.TryConstrain(cell.Tiles, dir))
                    {
                        neighbourConstrained = true;
                    }
                }
            }

            return neighbourConstrained;
        }

        public bool IsValidGrid()
        {
            // Grid is only valid if there is a miniumum of non-empty tiles once generation is complete?

            // Test that there are no disconnected loops/paths by performing a breadth first search
            // This checks for all tiles that can be visited from the cell immediately above the ghost box.
            var visited = gridGraph.BreadthFirstSearch(graphSearchStartTile);

            // If the number of visited tiles is equal to the number of non-empty tiles then conclude
            // that all visitable tiles can be reached.
            // If not then return false which will trigger a fresh generation.
            // (Alternative test for this?)
            return visited.Count == NonEmptyTileCount;
        }

        public IEnumerator CompleteGrid(float stepPause)
        {
            onStatusUpdate?.Invoke("Completing Grid");

            for (int x = genWidth - 1, z = genWidth; x >= 0; x--, z++)
            {
                for (var y = 0; y < height; y++)
                {
                    var cellToMirror = GetCell(x, y);
                    var mirroredTile = tilesConfig.GetMirroredTile(cellToMirror.Tiles[0]);

                    cellArray[y, z].SetTile(mirroredTile);

                    yield return new WaitForSeconds(stepPause);
                }
            }

            

            yield return AddOutsideEdgesToGrid(stepPause);

            AddGhostBoxEdges();

            isComplete = true;
        }

        public IEnumerator AddOutsideEdgesToGrid(float stepPause)
        {
            onStatusUpdate?.Invoke("Adding outside edges and ghost box to grid");

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

                yield return new WaitForSeconds(stepPause);

            } while (current != start);
        }

        private void AddGhostBoxEdges()
        {
            // ghostbox entrance left
            var cell = cellArray[middle + 1, genWidth - 1];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_LEFT_ENTRANCE);

            // ghostbox entrance right
            cell = cellArray[middle + 1, genWidth];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_RIGHT_ENTRANCE);

            // ghost box top left
            cell = cellArray[middle + 1, genWidth - 2];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_TOP_LEFT);

            // ghost box top right
            cell = cellArray[middle + 1, genWidth + 1];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_TOP_RIGHT);

            // ghost box left side
            cell = cellArray[middle, genWidth - 2];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.RIGHT_SIDE);

            // ghost box bottom left
            cell = cellArray[middle - 1, genWidth - 2];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_BOTTOM_LEFT);

            // ghost box bottom right
            cell = cellArray[middle - 1, genWidth + 1];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.GHOST_BOTTOM_RIGHT);

            // ghost box right side
            cell = cellArray[middle, genWidth + 1];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.LEFT_SIDE);

            // ghost box bottom side
            cell = cellArray[middle - 1, genWidth - 1];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.TOP_SIDE);
            cell = cellArray[middle - 1, genWidth];
            cell.CollapsedTile.AddDoubleEdge(SPRITEPART_SECTION.TOP_SIDE);
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

        private void AddOutsideEdgesToGrid()
        {
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