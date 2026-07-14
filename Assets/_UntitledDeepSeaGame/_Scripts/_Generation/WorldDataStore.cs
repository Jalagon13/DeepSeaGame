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

        public ushort[,] FgTileData { get; private set; }
        public ushort[,] BgTileData { get; private set; }
        private readonly HashSet<int> _naturalBackgroundTiles = new();
        private readonly HashSet<int> _underwaterAirTiles = new();
        private int _seaLevelY;
        
        private Dictionary<Vector2Int, TileSO> _activeMultiTileObjects;
        public IReadOnlyDictionary<Vector2Int, TileSO> ActiveMultiTileObjects => _activeMultiTileObjects;

        public int Width => FgTileData?.GetLength(0) ?? 0;
        public int Height => FgTileData?.GetLength(1) ?? 0;
        public int SeaLevelY => _seaLevelY;

        public void Initialize(int width, int height, int seaLevelY)
        {
            FgTileData = new ushort[width, height];
            BgTileData = new ushort[width, height];
            
            _seaLevelY = Mathf.Clamp(seaLevelY, 1, Mathf.Max(1, height - 1));
            _naturalBackgroundTiles.Clear();
            _underwaterAirTiles.Clear();
            _activeMultiTileObjects = new();


            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    FgTileData[x, y] = GameDataRegistry.INVALID_ID;
                    BgTileData[x, y] = GameDataRegistry.INVALID_ID;
                }
            }
        }
        
        public bool IsAtmosphereZone(int y)
        {
            return y >= _seaLevelY && y < Height;
        }
        
        public bool IsThereForegroundTile(int x, int y)
        {
            return GetTileId(x, y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID;
        }

        public bool IsOceanZone(int y)
        {
            return y >= 0 && y < _seaLevelY;
        }

        public void SetUnderwaterAir(int x, int y, bool value)
        {
            if (!IsInBounds(x, y))
            {
                return;
            }

            if (!IsOceanZone(y))
            {
                return;
            }

            if (value && GetTileId(x, y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID)
            {
                return;
            }
            
            int index = GetTileIndex(x, y);
            bool changed = value ? _underwaterAirTiles.Add(index) : _underwaterAirTiles.Remove(index);
            if (changed)
            {
                TileChanged?.Invoke(new Vector2Int(x, y), GameDataRegistry.INVALID_ID, GameDataRegistry.INVALID_ID, WorldTm.AirTilemap);
            }
        }
        
        public bool IsAirAt(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            return IsAtmosphereZone(y) || IsUnderwaterAirAt(x, y);
        }

        public bool IsUnderwaterAirAt(int x, int y)
        {
            if (!IsInBounds(x, y) || !IsOceanZone(y))
            {
                return false;
            }

            return _underwaterAirTiles.Contains(GetTileIndex(x, y));
        }

        public bool IsWaterCell(int x, int y)
        {
            if (!IsInBounds(x, y) || !IsOceanZone(y))
            {
                return false;
            }

            return GetTileId(x, y, WorldTm.ForegroundTilemap) == GameDataRegistry.INVALID_ID && !IsUnderwaterAirAt(x, y);
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

            // Notify the renderer to remove the GameObject and clean up the registry
            MultiTileChanged?.Invoke(anchor, multiTileSO, false);
            _activeMultiTileObjects.Remove(anchor);
        }

        public void SetTileId(int x, int y, ushort tileId, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                Debug.LogWarning($"Failed to set tile at ({x}, {y}) on {targetMap} because it is out of bounds.");
                return;
            }

            ushort[,] data = targetMap == WorldTm.ForegroundTilemap ? FgTileData : BgTileData;
            ushort previousTileId = data[x, y];
            if (previousTileId == tileId)
            {
                return;
            }

            data[x, y] = tileId;

            // Check for naturally generating walls if so register them if not remove it.
            if (targetMap == WorldTm.BackgroundTilemap)
            {
                int index = GetTileIndex(x, y);
                if (tileId != GameDataRegistry.INVALID_ID && !WorldManager.Instance.IsWorldReady)
                {
                    _naturalBackgroundTiles.Add(index);
                }
                else
                {
                    _naturalBackgroundTiles.Remove(index);
                }
            }

            TileChanged?.Invoke(new Vector2Int(x, y), previousTileId, tileId, targetMap);

            if (targetMap == WorldTm.ForegroundTilemap)
            {
                if(tileId != GameDataRegistry.INVALID_ID)
                {
                    ClearUnderwaterAirSilently(x, y, true);
                }
                else
                {
                    TryFloodConnectedAirPockets(x, y);
                }
            }
        }

        private void TryFloodConnectedAirPockets(int x, int y)
        {
            // Only relevant in the ocean zone
            if (!IsOceanZone(y))
            {
                return;
            }

            // Check the 4 cardinal neighbors for an air tile touching a water tile
            Vector2Int[] neighbors = new Vector2Int[]
            {
                new Vector2Int(x, y - 1), // down
                new Vector2Int(x, y + 1), // up
                new Vector2Int(x - 1, y), // left
                new Vector2Int(x + 1, y)  // right
            };

            bool foundAirTouchingWater = false;
            Vector2Int startAirCell = Vector2Int.zero;

            foreach (Vector2Int n in neighbors)
            {
                if (!IsInBounds(n.x, n.y) || !IsOceanZone(n.y))
                {
                    continue;
                }

                // Check if this neighbor is an air tile
                if (IsUnderwaterAirAt(n.x, n.y))
                {
                    // Check if any of *its* neighbors is a water cell (open water, not air)
                    Vector2Int[] subNeighbors = new Vector2Int[]
                    {
                        new Vector2Int(n.x, n.y - 1),
                        new Vector2Int(n.x, n.y + 1),
                        new Vector2Int(n.x - 1, n.y),
                        new Vector2Int(n.x + 1, n.y)
                    };

                    foreach (Vector2Int sn in subNeighbors)
                    {
                        if (!IsInBounds(sn.x, sn.y) || !IsOceanZone(sn.y))
                        {
                            continue;
                        }

                        if (IsWaterCell(sn.x, sn.y))
                        {
                            foundAirTouchingWater = true;
                            startAirCell = n;
                            break;
                        }
                    }

                    if (foundAirTouchingWater)
                    {
                        break;
                    }
                }
            }

            if (!foundAirTouchingWater)
            {
                return;
            }

            // BFS flood fill from the start air cell to fill the entire connected air pocket with water
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<int> visited = new HashSet<int>();

            queue.Enqueue(startAirCell);
            visited.Add(GetTileIndex(startAirCell.x, startAirCell.y));

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                // Convert this air cell to water by removing it from the underwater air set
                int index = GetTileIndex(current.x, current.y);
                if (_underwaterAirTiles.Remove(index))
                {
                    TileChanged?.Invoke(new Vector2Int(current.x, current.y), GameDataRegistry.INVALID_ID, GameDataRegistry.INVALID_ID, WorldTm.AirTilemap);
                }

                // Check 4 cardinal neighbors for more air cells to flood
                Vector2Int[] dirs = new Vector2Int[]
                {
                    new Vector2Int(current.x, current.y - 1),
                    new Vector2Int(current.x, current.y + 1),
                    new Vector2Int(current.x - 1, current.y),
                    new Vector2Int(current.x + 1, current.y)
                };

                foreach (Vector2Int next in dirs)
                {
                    if (!IsInBounds(next.x, next.y) || !IsOceanZone(next.y))
                    {
                        continue;
                    }

                    int nextIndex = GetTileIndex(next.x, next.y);
                    if (visited.Contains(nextIndex))
                    {
                        continue;
                    }

                    // Only flood cells that are underwater air (not solid foreground tiles)
                    if (_underwaterAirTiles.Contains(nextIndex))
                    {
                        visited.Add(nextIndex);
                        queue.Enqueue(next);
                    }
                }
            }
        }

        private bool ClearUnderwaterAirSilently(int x, int y, bool notify)
        {
            if (!IsInBounds(x, y) || !IsOceanZone(y))
            {
                return false;
            }

            bool removed = _underwaterAirTiles.Remove(GetTileIndex(x, y));
            if (removed && notify)
            {
                TileChanged?.Invoke(new Vector2Int(x, y), GameDataRegistry.INVALID_ID, GameDataRegistry.INVALID_ID, WorldTm.AirTilemap);
            }

            return removed;
        }

        public bool IsPlayerPlacedBackgroundAt(int x, int y)
        {
            if (!IsInBounds(x, y)) return false;
            
            ushort tileId = GetTileId(x, y, WorldTm.BackgroundTilemap);
            if (tileId == GameDataRegistry.INVALID_ID) return false;
            
            int index = GetTileIndex(x, y);
            return !_naturalBackgroundTiles.Contains(index);
        }

        public ushort GetTileId(int x, int y, WorldTm targetMap = WorldTm.ForegroundTilemap)
        {
            if (!IsInBounds(x, y))
            {
                return GameDataRegistry.INVALID_ID;
            }

            return targetMap == WorldTm.ForegroundTilemap ? FgTileData[x, y] : BgTileData[x, y];
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private int GetTileIndex(int x, int y)
        {
            return (y * Width) + x;
        }
    }
}
