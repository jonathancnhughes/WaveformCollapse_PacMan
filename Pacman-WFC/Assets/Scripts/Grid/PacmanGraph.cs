using System.Collections.Generic;

namespace JFlex.PacmanWFC.Data
{
    public class PacmanGraph
    {
        private readonly int width;
        private readonly int height;
        private readonly CellObj startCell;

        public PacmanGraph(int height, int width, CellObj startCell)
        {
            this.height = height;
            this.width = width;
            this.startCell = startCell;
        }

        public bool IsStillConnected(CellObj[,] cells)
        {
            int activeCount = 0;

            // Count active cells
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsActive(cells[y,x]))
                    {
                        activeCount++;
                    }
                }
            }

            // Nothing active = trivially connected
            if (activeCount == 0)
            {
                return true;
            }                

            List<CellObj> visited = new();
            Queue<CellObj> queue = new();

            queue.Enqueue(startCell);

            while (queue.Count > 0)
            {
                CellObj node = queue.Dequeue();

                if (!visited.Contains(node))
                {
                    visited.Add(node);

                    foreach (var dir in DirectionExtensions.AllDirections)
                    {
                        int nx = node.X, ny = node.Y;

                        switch (dir)
                        {
                            case Direction.Up: ny++; break;
                            case Direction.Down: ny--; break;
                            case Direction.Left: nx--; break;
                            case Direction.Right: nx++; break;
                        }

                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;

                        CellObj neighbour = cells[ny, nx];

                        // do not queue if:
                        // 1. already visited
                        // 2. not a valid tile to visit i.e. an empty tile
                        // 3. if collapsed and no connection to neighbour
                        // else enqueue it!

                        if (visited.Contains(neighbour))
                        {
                            continue;
                        }

                        if (!IsActive(neighbour))
                        {
                            continue;
                        }

                        if (node.IsCollapsed)
                        {
                            if (neighbour.IsCollapsed)
                            {
                                // if current is collapsed and neighbour is collapsed
                                // -  test if share an edge to neighbour
                                if (!node.CollapsedTile.SharedEdgeWithNeighbour(neighbour.CollapsedTile, dir))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                // if current is collapsed and neighbour is not collapsed
                                // - test if current has a connection in this direction
                                if (!node.CollapsedTile.HasConnection(dir))
                                {
                                    continue;
                                }
                            }
                        }
                        else if (neighbour.IsCollapsed)
                        {
                            // if current is not collapsed and neighbour is collapsed
                            // - test if neighbour has opposite connection to this direction
                            if (!neighbour.CollapsedTile.HasConnection(dir.Opposite()))
                            {
                                continue;
                            }
                            
                        }

                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited.Count == activeCount;
        }

        private bool IsActive(CellObj cell)
        {
            if (cell.Tiles.Count == 0)
            {
                return false;
            }

            if (cell.IsCollapsed && !cell.CollapsedTile.IsEmptyTile)
            {
                return true;
            }

            for (var i = 0; i < cell.TileCount; i++)
            {
                if (cell.Tiles[i].Connections != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}