using JFlex.PacmanWFC.Data;
using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC
{
    [System.Serializable]
    public class CellObj : CellObjBase
    {
        [SerializeField]
        private Direction validDirections = Direction.None;
        public Direction ValidDirections => validDirections;

        private List<TileData> tiles;
        public List<TileData> Tiles => tiles;

        public override int TileCount => (tiles == null || isCollapsed) ? 0 : tiles.Count;

        public SPRITEPART_TYPE[] SpriteParts
        {
            get
            {
                if (CollapsedTile == null)
                {
                    return new SPRITEPART_TYPE[0];
                }

                return collapsedTile.SpriteParts;
            }
        }

        public CellObj(int x, int y) : base(x, y)
        { }

        public CellObj(int x, int y, List<TileData> tileList, Direction directions) : base(x, y)
        {
            tiles = new List<TileData>(tileList);
            validDirections = directions;

            isCollapsed = false;

            RemoveInvalidTiles();
        }

        public override string ToString()
        {
            return $"Cell ({x},{y})";
        }

        public void ResetCell(List<TileData> tileList)
        {
            tiles = new List<TileData>(tileList);

            RemoveInvalidTiles();
            isCollapsed = false;
        }

        private void RemoveInvalidTiles()
        {
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                var tile = tiles[i];

                foreach (var direction in DirectionExtensions.AllDirections)
                {
                    if ((validDirections & direction) == 0 && tile.HasConnection(direction))
                    {
                        tiles.Remove(tile);
                    }
                }
            }
        }

        public override void SetTile(TileData tile, bool doNotMirror = false)
        {
            base.SetTile(tile, doNotMirror);
            tiles = new List<TileData>() { tile };
            validDirections = tile.Connections;
        }

        public override bool TryCollapse()
        {
            if (tiles.Count == 0)
            {
                Debug.Log("No tiles possible for this cell!");
                return false;
            }

            var idx = Random.Range(0, tiles.Count);

            collapsedTile = tiles[idx];
            tiles = new List<TileData>() { collapsedTile };


            isCollapsed = true;

            return true;
        }

        public bool TryConstrain(List<TileData> neighbourOptions, Direction direction)
        {
            var reduced = false;

            // Update the options for this tile based on its updated neighbour.
            if (isCollapsed)
            {
                return false;
            }

            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                var tile = tiles[i];
                var foundMatch = false;

                foreach (var neighbour in neighbourOptions)
                {
                    foundMatch = tile.CanConnectToNeighbour(neighbour, direction);
                    if (foundMatch)
                    {
                        break;
                    }
                }

                if (!foundMatch)
                {
                    tiles.Remove(tile);
                    reduced = true;
                }
            }

            return reduced;
        }

        public override List<TileData> GetTiles()
        {
            return Tiles;
        }
    }
}