using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HabitatCore : MonoBehaviour
    {
        [SerializeField] private int _maxTileDetection = 40;
        [SerializeField] private float _drainInterval = 5f;

        private float _timer = 0f;

        private void Update()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            
            if (WorldManager.Instance == null) return;
            
            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;
            if (dataStore == null || !WorldManager.Instance.IsWorldReady) return;

            if (IsInWater())
            {
                _timer += Time.deltaTime;
                if (_timer >= _drainInterval)
                {
                    _timer = 0f;
                    TryDrainAttempt();
                }
            }
        }

        private bool IsInWater()
        {
            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;
            if (dataStore == null) return false;

            Vector2Int anchor = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            Vector2Int size = new Vector2Int(2, 2);
            if (dataStore.ActiveMultiTileObjects.TryGetValue(anchor, out TileSO tileSO))
            {
                size = tileSO.Size;
            }

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int pos = new Vector2Int(anchor.x + i, anchor.y + j);
                    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int neighbor = pos + dir;
                        // Skip if it's within the footprint itself
                        if (neighbor.x >= anchor.x && neighbor.x < anchor.x + size.x &&
                            neighbor.y >= anchor.y && neighbor.y < anchor.y + size.y)
                        {
                            continue;
                        }

                        if (dataStore.IsWaterCell(neighbor.x, neighbor.y))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void TryDrainAttempt()
        {
            Debug.Log($"Attempting drain attempt from habitat core position");

            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;

            if (dataStore == null) return;

            Vector2Int anchor = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            Vector2Int size = new Vector2Int(2, 2);
            if (dataStore.ActiveMultiTileObjects.TryGetValue(anchor, out TileSO tileSO))
            {
                size = tileSO.Size;
            }

            // 1. Initialize BFS flood fill
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();

            // Add footprint to visited so we don't traverse into the core itself
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    visited.Add(new Vector2Int(anchor.x + i, anchor.y + j));
                }
            }

            // 2. Enqueue all adjacent water cells in the ocean zone that have player-placed background
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int pos = new Vector2Int(anchor.x + i, anchor.y + j);
                    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int neighbor = pos + dir;
                        if (!visited.Contains(neighbor))
                        {
                            if (dataStore.IsWaterCell(neighbor.x, neighbor.y) && dataStore.IsPlayerPlacedBackgroundAt(neighbor.x, neighbor.y))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            // If no valid water cell adjacent, return
            if (queue.Count == 0)
            {
                return;
            }

            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int dir in neighbors)
                {
                    Vector2Int next = current + dir;

                    // If we hit the bounds of the world, we assume it's not an enclosed pocket
                    if (!dataStore.IsInBounds(next.x, next.y))
                    {
                        OnDrainFailure();
                        return;
                    }

                    // We traverse the connected void (water or air). 
                    // If a cell in the void has a natural wall or no wall, it's a "leak".
                    bool isSolidBoundary = dataStore.GetTileId(next.x, next.y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID;

                    if (!isSolidBoundary)
                    {
                        if (!dataStore.IsOceanZone(next.y) || !dataStore.IsPlayerPlacedBackgroundAt(next.x, next.y))
                        {
                            OnDrainFailure();
                            return;
                        }

                        if (!visited.Contains(next))
                        {
                            visited.Add(next);

                            // If we exceed the capacity, it's a failure.
                            // Note: we subtract the footprint tiles from visited.Count to get the actual empty cells detected.
                            int nonFootprintCount = visited.Count - size.x * size.y;
                            if (nonFootprintCount > _maxTileDetection)
                            {
                                OnDrainFailure();
                                return;
                            }

                            queue.Enqueue(next);
                        }
                    }
                }
            }

            // BFS finished and the entire area is within capacity
            OnDrainSuccess(visited);
        }

        private void OnDrainSuccess(IEnumerable<Vector2Int> visited)
        {
            Debug.Log("Sponge Success: Enclosed area found within capacity.");

            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;
            int count = 0;

            foreach (Vector2Int pos in visited)
            {
                if (dataStore.IsWaterCell(pos.x, pos.y))
                {
                    dataStore.SetUnderwaterAir(pos.x, pos.y, true);
                    count++;
                }
            }

            Debug.Log($"Sponge Success: Enclosed area found. {count} air tiles created.");
        }

        private void OnDrainFailure()
        {
            Debug.Log("Sponge Failure: Area too large or not fully enclosed.");
        }
    }
}
