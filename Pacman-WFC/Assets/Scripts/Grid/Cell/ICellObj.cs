using JFlex.PacmanWFC.Data;
using System.Collections.Generic;

namespace JFlex.PacmanWFC
{
    public interface ICellObj
    {
        public List<TileData> GetTiles();

        public bool TryCollapse();

        public void SetTile(TileData tile, bool isSpecial);

        public int TileCount { get; }
    }
}