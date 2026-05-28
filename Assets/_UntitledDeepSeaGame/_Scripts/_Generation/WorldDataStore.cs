using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldDataStore : MonoBehaviour
    {
        public event Action<Vector2Int, ushort, ushort, WorldTm> TileChanged;

        private ushort[,] _fgTileData;
        private ushort[,] _bgTileData;
        private bool[,] _airTileData;

        public int Width => _fgTileData?.GetLength(0) ?? 0;
        public int Height => _fgTileData?.GetLength(1) ?? 0;

        public void Initialize(int width, int height)
        {
            _fgTileData = new ushort[width, height];
            _bgTileData = new ushort[width, height];
            _airTileData = new bool[width, height];
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _fgTileData[x, y] = GameDataRegistry.INVALID_ID;
                    _bgTileData[x, y] = GameDataRegistry.INVALID_ID;
                    _airTileData[x, y] = false;
                }
            }
        }
        
        public void SetAirValue(int x, int y, bool value)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }
            
            _airTileData[x, y] = value;
            TileChanged?.Invoke(new Vector2Int(x, y), GameDataRegistry.INVALID_ID, GameDataRegistry.INVALID_ID, WorldTm.AirTilemap);
        }
        
        public bool IsAirAt(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            return _airTileData[x, y];
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

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
