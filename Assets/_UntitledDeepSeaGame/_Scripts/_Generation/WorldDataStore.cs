using System;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    // The source of truth for the entire world
    public class WorldDataStore : MonoBehaviour
    {
        public event Action<Vector2Int, ushort, ushort, WorldTm> TileChanged;
        public event Action<Vector2Int, TileSO, bool> MultiTileChanged;

        private ushort[,] _fgTileData;
        private ushort[,] _bgTileData;
        private bool[,] _airTileData;
        
        private Dictionary<Vector2Int, TileSO> _activeMultiTileObjects;
        public IReadOnlyDictionary<Vector2Int, TileSO> ActiveMultiTileObjects => _activeMultiTileObjects;

        public int Width => _fgTileData?.GetLength(0) ?? 0;
        public int Height => _fgTileData?.GetLength(1) ?? 0;

        public void Initialize(int width, int height)
        {
            _fgTileData = new ushort[width, height];
            _bgTileData = new ushort[width, height];
            _airTileData = new bool[width, height];
            _activeMultiTileObjects = new();


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
        
        public void SetMultiTile(int x, int y, TileSO tile)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            // Register the anchor so the renderer knows where to spawn the multi-tile entity
            Vector2Int anchor = new Vector2Int(x, y);
            _activeMultiTileObjects[anchor] = tile;

            // Fill the entire footprint in the tile data array with the actual Tile ID.
            // This allows the Mini-Map to render the full shape and ensures placement logic sees the space as occupied.
            ushort tileId = GameDataRegistry.Instance.GetTileIdFromTileSO(tile);
            for (int i = 0; i < tile.Size.x; i++)
            {
                for (int j = 0; j < tile.Size.y; j++)
                {
                    // Calling SetTileId triggers the TileChanged event for every coordinate in the footprint
                    SetTileId(x + i, y + j, tileId, WorldTm.ForegroundTilemap);
                }
            }

            // Notify the renderer to spawn the GameObject at the anchor
            MultiTileChanged?.Invoke(anchor, tile, true);
        }
        
        public void DestroyMultiTile(int x, int y)
        {
            if (!IsInBounds(x, y)) return;

            Vector2Int anchor = Vector2Int.zero;
            TileSO multiTileSO = null;
            bool found = false;

            // Search the registry to find which multi-tile footprint contains these coordinates
            foreach (var kvp in _activeMultiTileObjects)
            {
                Vector2Int pos = kvp.Key;
                TileSO so = kvp.Value;
                if (x >= pos.x && x < pos.x + so.Size.x && y >= pos.y && y < pos.y + so.Size.y)
                {
                    anchor = pos;
                    multiTileSO = so;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning($"Could not break a multitile here at ({x}, {y}). This should not be possible, It should exist");
                return;
            }

            // Clear every tile in the multi-tile's footprint
            for (int i = 0; i < multiTileSO.Size.x; i++)
            {
                for (int j = 0; j < multiTileSO.Size.y; j++)
                {
                    // SetTileId will trigger the TileChanged event and clean up the anchor registry automatically
                    SetTileId(anchor.x + i, anchor.y + j, GameDataRegistry.INVALID_ID, WorldTm.ForegroundTilemap);
                }
            }
        }

        public void SetTileId(int x, int y, ushort tileId, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                Debug.LogWarning($"Failed to set tile at ({x}, {y}) on {targetMap} because it is out of bounds.");
                return;
            }

            ushort[,] data = targetMap == WorldTm.ForegroundTilemap ? _fgTileData : _bgTileData;
            ushort previousTileId = data[x, y];
            if (previousTileId == tileId)
            {
                return;
            }

            data[x, y] = tileId;
            TileChanged?.Invoke(new Vector2Int(x, y), previousTileId, tileId, targetMap);

            // Check for exposed air tiles if a foreground tile was broken
            if (targetMap == WorldTm.ForegroundTilemap && tileId == GameDataRegistry.INVALID_ID && !IsAirAt(x, y))
            {
                CheckForExposedAir(x, y);
            }
        }

        public ushort GetTileId(int x, int y, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                return GameDataRegistry.INVALID_ID;
            }

            return targetMap == WorldTm.ForegroundTilemap ? _fgTileData[x, y] : _bgTileData[x, y];
        }

        private void CheckForExposedAir(int x, int y)
        {
            Vector2Int[] neighbors = { new(x, y + 1), new(x, y - 1), new(x - 1, y), new(x + 1, y) };
            bool hasWaterNeighbor = false;

            foreach (var pos in neighbors)
            {
                // A "water" tile is an empty foreground tile that is not air
                if (GetTileId(pos.x, pos.y, WorldTm.ForegroundTilemap) == GameDataRegistry.INVALID_ID && !IsAirAt(pos.x, pos.y))
                {
                    hasWaterNeighbor = true;
                    break;
                }
            }

            if (hasWaterNeighbor)
            {
                foreach (var pos in neighbors)
                {
                    if (IsAirAt(pos.x, pos.y))
                    {
                        OnAirTileExposed(pos.x, pos.y);
                    }
                }
            }
            else
            {
                SetAirValue(x, y, true);
            }
        }

        private void OnAirTileExposed(int startX, int startY)
        {
            Debug.Log($"Air pocket exposed at ({startX}, {startY})! Filling with water...");

            Queue<Vector2Int> queue = new();
            
            // Start the flood fill by clearing the first detected air tile
            SetAirValue(startX, startY, false);
            queue.Enqueue(new Vector2Int(startX, startY));

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (var dir in neighbors)
                {
                    Vector2Int next = current + dir;
                    
                    if (IsAirAt(next.x, next.y))
                    {
                        // Setting to false immediately prevents the tile from being re-added to the queue
                        SetAirValue(next.x, next.y, false);
                        queue.Enqueue(next);
                    }
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
