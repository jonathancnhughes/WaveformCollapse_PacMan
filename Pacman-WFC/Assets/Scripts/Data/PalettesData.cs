using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [CreateAssetMenu(fileName = "PalletesData", menuName = "Pacman-WFC/PalletesData")]
    public class PalettesData : ScriptableObject
    {
        [SerializeField]
        private TilePaletteDataDictionary paletteData;
        public Dictionary<TILE_PALETTE, SpritePartMapping> Data => paletteData.Dictionary;

        public SpritePartMapping GetSpritePartMapping(TILE_PALETTE palette)
        {
            if (paletteData.TryGetValue(palette, out var spritePartMapping)) 
            {
                return spritePartMapping;
            }

            return null;
        }
    }
}