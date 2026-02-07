namespace JFlex.PacmanWFC
{
    public class CellGridBase<T> : ICellGrid<T> where T : CellObjBase
    {
        private ICellObj[,] cellArray;
        private int height;
        private int width;
        private int genWidth;
        public int WidthToGenerate => genWidth;

        public CellGridBase(int height, int width)
        {
            cellArray = new ICellObj[height, width];
            this.height = height;

            this.width = width;
            // Width needs to be even so it can be halved to generate the left half of the grid.
            if (this.width % 2 != 0)
            {
                this.width++;
            }
            genWidth = this.width / 2;

        }

        public bool CellAlreadyPlaced(int x, int y)
        {
            return (cellArray[y, x] != null);
        }

        public T GetCell(int x, int y)
        {
            return (T)cellArray[y, x];
        }

        public bool TryGetCellNeighbour(T cell, Direction direction, out T neighbour)
        {
            neighbour = default(T);
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
    }
}