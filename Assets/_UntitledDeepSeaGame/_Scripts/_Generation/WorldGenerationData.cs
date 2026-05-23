using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldGenerationData : MonoBehaviour
    {
        [SerializeField] 
        private int _worldWidth, _worldHeight;

        [field: SerializeField]
        public string Seed { get; private set; }

        public ushort[,] TileData;

        public void ResetData()
        {
            TileData = new ushort[_worldWidth, _worldHeight];
        }

        public virtual void SetTileData(int x, int y, TileSO tileSO)
        {
            TileData[x, y] = GameDataRegistry.Instance.GetTileIdFromTileSO(tileSO);
        }

        public bool IsInBounds(int x, int y)
        {
            int width = TileData.GetLength(0);
            int height = TileData.GetLength(1);
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
}