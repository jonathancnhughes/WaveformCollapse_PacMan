using JFlex.Core;
using System;

namespace JFlex.PacmanWFC.Data
{
    public enum TILE_PALETTE
    {
        BLUE_RED,
        RED_YELLOW,
        BLACK_BLUE,
        GREEN_WHITE
    }

    [Serializable]
    public class TilePaletteDataDictionary : EnumSerializedDictionary<TILE_PALETTE, SpritePartMapping>
    {
    }
}