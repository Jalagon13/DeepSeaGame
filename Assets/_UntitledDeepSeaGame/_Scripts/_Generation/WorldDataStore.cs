using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldDataStore : MonoBehaviour
    {
        public event Action<Vector2Int, ushort, ushort> TileChanged;

        private ushort[,] _tileData;

        public int Width => _tileData?.GetLength(0) ?? 0;
        public int Height => _tileData?.GetLength(1) ?? 0;

        public void Initialize(int width, int height, ushort defaultTileId = GameDataRegistry.INVALID_ID)
        {
            _tileData = new ushort[width, height];
            Fill(defaultTileId);
        }

        public void Fill(ushort tileId)
        {
            if (_tileData == null)
            {
                return;
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _tileData[x, y] = tileId;
                }
            }
        }

        public void Clear()
        {
            Fill(GameDataRegistry.INVALID_ID);
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public ushort GetTileId(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return GameDataRegistry.INVALID_ID;
            }

            return _tileData[x, y];
        }

        public void SetTileId(int x, int y, ushort tileId)
        {
            if (!TrySetTileId(x, y, tileId))
            {
                Debug.LogWarning($"Failed to set tile at ({x}, {y}) because it is out of bounds.");
            }
        }

        public bool TrySetTileId(int x, int y, ushort tileId)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            ushort previousTileId = _tileData[x, y];
            if (previousTileId == tileId)
            {
                return true;
            }

            _tileData[x, y] = tileId;
            TileChanged?.Invoke(new Vector2Int(x, y), previousTileId, tileId);
            return true;
        }
    }
}
