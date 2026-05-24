using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldDataStore : MonoBehaviour
    {
        public event Action<Vector2Int, ushort, ushort, WorldTm> TileChanged;

        private ushort[,] _fgTileData;
        private ushort[,] _bgTileData;

        public int Width => _fgTileData?.GetLength(0) ?? 0;
        public int Height => _fgTileData?.GetLength(1) ?? 0;

        public void Initialize(int width, int height)
        {
            _fgTileData = new ushort[width, height];
            _bgTileData = new ushort[width, height];
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _fgTileData[x, y] = GameDataRegistry.INVALID_ID;
                    _bgTileData[x, y] = GameDataRegistry.INVALID_ID;
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public ushort GetTileId(int x, int y, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                return GameDataRegistry.INVALID_ID;
            }

            return targetMap == WorldTm.ForegroundTilemap ? _fgTileData[x, y] : _bgTileData[x, y];
        }

        public void SetTileId(int x, int y, ushort tileId, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!TrySetTileId(x, y, tileId, targetMap))
            {
                Debug.LogWarning($"Failed to set tile at ({x}, {y}) on {targetMap} because it is out of bounds.");
            }
        }

        public bool TrySetTileId(int x, int y, ushort tileId, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            ushort[,] data = targetMap == WorldTm.ForegroundTilemap ? _fgTileData : _bgTileData;
            ushort previousTileId = data[x, y];
            if (previousTileId == tileId)
            {
                return true;
            }

            data[x, y] = tileId;
            TileChanged?.Invoke(new Vector2Int(x, y), previousTileId, tileId, targetMap);
            return true;
        }
    }
}
