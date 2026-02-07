using JFlex.Core;
using JFlex.PacmanWFC;
using JFlex.PacmanWFC.Data;

public class WeightedCellGrid<T> : ICellGrid<T> where T : WeightedCellObj
{
    private WeightedCellObj[,] cellArray;
    private int height;
    private int width;
    private int genWidth;

    public int WidthToGenerate => genWidth;

    private WeightedRandomTable<TileData> insideTiles;
    private WeightedRandomTable<TileData> topCornerTiles;
    private WeightedRandomTable<TileData> bottomCornerTiles;
    private WeightedRandomTable<TileData> leftEdgeTiles;
    private WeightedRandomTable<TileData> topEdgeTiles;
    private WeightedRandomTable<TileData> bottomEdgeTiles;

    public WeightedCellGrid(int height, int width, 
        WeightedRandomTable<TileData> insideTiles,
        WeightedRandomTable<TileData> topCornerTiles,
        WeightedRandomTable<TileData> bottomCornerTiles,
        WeightedRandomTable<TileData> leftEdgeTiles,
        WeightedRandomTable<TileData> topEdgeTiles,
        WeightedRandomTable<TileData> bottomEdgeTiles)
    {
        cellArray = new WeightedCellObj[height, width];

        this.height = height;

        this.width = width;
        // Width needs to be even so it can be halved to generate the left half of the grid.
        if (this.width % 2 != 0)
        {
            this.width++;
        }
        genWidth = this.width / 2;

        this.insideTiles = insideTiles;
        this.topCornerTiles = topCornerTiles;
        this.bottomCornerTiles = bottomCornerTiles;
        this.leftEdgeTiles = leftEdgeTiles;
        this.topEdgeTiles = topEdgeTiles;
        this.bottomEdgeTiles = bottomEdgeTiles;
    }

    public void AddCellObjAndInit(WeightedCellObj cellObj, Direction directions, int x, int y, bool isOpenCell)
    {
        var weightedTiles = CalculateRequiredTileSet(x, y);
        cellObj.Init(weightedTiles, directions, x, y, isOpenCell);

        cellArray[y, x] = cellObj;
    }

    public bool CellAlreadyPlaced(int x, int y)
    {
        return (cellArray[y, x] != null);
    }

    public T GetCell(int x, int y)
    {
        return (T)cellArray[y, x];
    }

    private WeightedRandomTable<TileData> CalculateRequiredTileSet(int x, int y)
    {
        if (x == 0)
        {
            if (y == 0)
            {
                return bottomCornerTiles;
            }
            else if (y == height - 1)
            {
                return topCornerTiles;
            }

            return leftEdgeTiles;
        }
        else if (y == 0)
        {
            return bottomEdgeTiles;
        }
        else if (y == height - 1)
        {
            return topEdgeTiles;
        }

        return insideTiles;
    }

    public void ResetCells()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < genWidth)
                {
                    var weightedTiles = CalculateRequiredTileSet(x, y);
                    cellArray[y, x].ResetCell(weightedTiles);
                }
                //else
                //{
                //    cellArray[y, x].ResetCell();
                //}
            }
        }
    }

    public bool TryGetCellNeighbour(T cell, Direction direction, out T neighbour)
    {
        throw new System.NotImplementedException();
    }
}
