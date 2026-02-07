using JFlex.Core;
using JFlex.PacmanWFC;
using JFlex.PacmanWFC.Data;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedCellObj : CellObjBase
{
    public int CellNumber;
    private bool isOpenCell;

    [SerializeField]
    private CellObj upNeighbour;
    [SerializeField]
    private CellObj downNeighbour;
    [SerializeField]
    private CellObj leftNeighbour;
    [SerializeField]
    private CellObj rightNeighbour;

    [SerializeField]
    private Direction validDirections = Direction.None;
    public Direction ValidDirections => validDirections;

    private WeightedRandomTable<TileData> tiles;
    public WeightedRandomTable<TileData> Tiles => tiles;

    public WeightedCellObj(int x, int y) : base(x, y)
    { }

    public void Init(WeightedRandomTable<TileData> tiles, Direction directions, int x, int y, bool isOpenCell)
    {
        this.tiles = tiles;

        validDirections = directions;

        this.isOpenCell = isOpenCell;

        isCollapsed = false;

        this.x = x; this.y = y;

        //UpdateNumberLabel(tiles.Count);
    }

    public void ResetCell(WeightedRandomTable<TileData> tileList)
    {
        tiles = tileList;

        isCollapsed = false;
        collapsedTile = null;

        //UpdateNumberLabel();

        //ResetCell();
    }

    //public void ResetCell()
    //{
    //    spriteRenderer.sprite = emptyCellSprite;
    //}
    public void SetTile(TileData tile, int x, int y, bool isSpecial = false)
    {
        collapsedTile = tile;

        isCollapsed = true;
        this.doNotMirror = isSpecial;

        //Draw();
    }

    //public void Draw()
    //{
    //    if (!isCollapsed)
    //    {
    //        return;
    //    }

    //    spriteRenderer.sprite = collapsedTile.Sprite;

    //    UpdateNumberLabel();
    //}

    //public bool TryCollapse()
    //{
    //    //Debug.Log($"Collapsing {name} ({CellNumber})");

    //    if (tiles.Count == 0)
    //    {
    //        Debug.Log("No tiles possible for this cell!");
    //        return false;
    //    }

    //    // Pick a tile randomly from the valid options. (Consider weights!)
    //    //var idx = Random.Range(0, tiles.Count);
    //    //var tmpTile = tiles[idx];

    //    var tmpTile = tiles.GetRandomItem();

    //    collapsedTile = tmpTile;
    //    //tiles = new List<Tile>() { tmpTile };

    //    isCollapsed = true;

    //    Draw();

    //    UpdateNumberLabel();

    //    return true;
    //}

    public bool TryConstrain(List<TileData> neighbourOptions, Direction direction)
    {
        var reduced = false;

        // Update the options for this tile based on its updated neighbour.
        if (isCollapsed)
        {
            return false;
        }

        bool LF_TestForMatch(TileData tile, TileData neighbour, Direction dir)
        {
            var foundMatch = false;

            //if (direction == Direction.Up)
            //{
            //    foundMatch = neighbour.HasUpConnection && tile.HasDownConnection ||
            //        !neighbour.HasUpConnection && !tile.HasDownConnection;
            //}
            //else if (direction == Direction.Down)
            //{
            //    foundMatch = neighbour.HasDownConnection && tile.HasUpConnection ||
            //        !neighbour.HasDownConnection && !tile.HasUpConnection;
            //}
            //else if (direction == Direction.Left)
            //{
            //    foundMatch = neighbour.HasLeftConnection && tile.HasRightConnection ||
            //        !neighbour.HasLeftConnection && !tile.HasRightConnection;
            //}
            //else if (direction == Direction.Right)
            //{
            //    foundMatch = neighbour.HasRightConnection && tile.HasLeftConnection ||
            //        !neighbour.HasRightConnection && !tile.HasLeftConnection;
            //}

            return foundMatch;
        }

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            var tile = tiles.Items[i];
            var foundMatch = false;

            foreach (var neighbour in neighbourOptions)
            {
                foundMatch = LF_TestForMatch(tile, neighbour, direction);
                if (foundMatch)
                {
                    break;
                }
            }

            if (!foundMatch)
            {
                tiles.RemoveFromTable(tile);
                reduced = true;
            }
        }

        //UpdateNumberLabel(tiles.Count);

        return reduced;
    }


    public void SetValidDirections(Direction directions)
    {
        validDirections = directions;
    }

    public void SetNeighbours(CellObj up, CellObj down, CellObj left, CellObj right)
    {
        upNeighbour = up;
        downNeighbour = down;
        leftNeighbour = left;
        rightNeighbour = right;
    }

    public CellObj GetNeighbouringCell(Direction direction)
    {
        CellObj nCell = null;
        switch (direction)
        {
            case Direction.Up:
                nCell = upNeighbour;
                break;
            case Direction.Down:
                nCell = downNeighbour;
                break;
            case Direction.Left:
                nCell = leftNeighbour;
                break;
            case Direction.Right:
                nCell = rightNeighbour;
                break;
        }

        return nCell;
    }
}