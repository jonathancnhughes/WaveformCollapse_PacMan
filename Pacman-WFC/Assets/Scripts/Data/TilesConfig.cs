using System.Collections.Generic;
using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [CreateAssetMenu(fileName = "TilesConfig", menuName = "Pacman-WFC/TilesConfig")]
    public class TilesConfig : ScriptableObject
    {
        [SerializeField]
        private List<TileData> tiles;
        public List<TileData> Tiles => CloneTiles(tiles);

        [Header("Special Tiles")]
        [SerializeField]
        private TileData emptyTileData;
        public TileData EmptyTile => emptyTileData.Clone();

        [SerializeField]
        private TileData tunnelTile;
        public TileData TunnelTile => tunnelTile.Clone();

        [Header("Ghost Box Tiles")]
        [SerializeField]
        private TileData ghostSpawnTile;
        public TileData GhostSpwnTile => ghostSpawnTile.Clone();

        [SerializeField]
        private List<TileData> spawnEntranceTiles;
        public List<TileData> SpawnEntranceTiles => CloneTiles(spawnEntranceTiles);

        [SerializeField]
        private List<TileData> aboveBoxTiles;
        public List<TileData> AboveBoxTiles => CloneTiles(aboveBoxTiles);

        [SerializeField]
        private List<TileData> belowBoxTiles;
        public List<TileData> BelowBoxTiles => CloneTiles(belowBoxTiles);

        [SerializeField]
        private List<TileData> sideBoxTiles;
        public List<TileData> SideBoxTiles => CloneTiles(sideBoxTiles);

        private List<TileData> CloneTiles(List<TileData> data)
        {
            List<TileData> tiles = new();

            foreach (var td in data)
            {
                tiles.Add(td.Clone());
            }

            return tiles;
        }

        public TileData GetMirroredTile(TileData original)
        {
            if (original.MirroredTile == null)
            {
                return Instantiate(original);
            }

            return Instantiate(original.MirroredTile);
        }
    }
}