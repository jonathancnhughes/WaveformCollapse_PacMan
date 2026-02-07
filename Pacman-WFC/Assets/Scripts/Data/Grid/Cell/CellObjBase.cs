using JFlex.PacmanWFC.Data;
using System.Collections.Generic;

namespace JFlex.PacmanWFC
{
    public class CellObjBase : ICellObj
    {
        protected int x;
        public int X => x;
        protected int y;
        public int Y => y;

        protected bool isCollapsed;
        public bool IsCollapsed => isCollapsed;

        protected bool doNotMirror;
        public bool DoNotMirror => doNotMirror;

        protected TileData collapsedTile;
        public TileData CollapsedTile => collapsedTile;

        public virtual int TileCount => 1;

        public CellObjBase(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual List<TileData> GetTiles()
        {
            return null;
        }

        public virtual void SetTile(TileData tile, bool doNotMirror)
        {
            collapsedTile = tile;
            isCollapsed = true;
            this.doNotMirror = doNotMirror;
        }

        public virtual bool TryCollapse()
        {
            return false;
        }
    }
}