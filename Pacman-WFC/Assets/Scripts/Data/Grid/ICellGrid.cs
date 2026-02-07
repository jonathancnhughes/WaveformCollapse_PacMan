namespace JFlex.PacmanWFC
{
    public interface ICellGrid<T> where T : CellObjBase
    {
        public abstract bool TryGetCellNeighbour(T cell, Direction direction, out T neighbour);

        public abstract T GetCell(int x, int y);

        public abstract bool CellAlreadyPlaced(int x, int y);
    }
}