using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "Habitat Core Behavior", menuName = "MultiTile/Lifecycle/HabitatCore")]
    public class HabitatCoreBehavior : MultiTileBehavior
    {
        [SerializeField] private int _maxTileDetection = 40;
        [SerializeField] private float _drainInterval = 5f;

        public override void Update(MultiTileInstance instance, WorldDataStore dataStore, float deltaTime)
        {
            if (instance == null || dataStore == null)
            {
                return;
            }

            instance.Timer += deltaTime;
            if (instance.Timer < _drainInterval)
            {
                return;
            }

            instance.Timer = 0f;

            if (!IsInWater(instance, dataStore))
            {
                return;
            }

            TryDrainAttempt(instance, dataStore);
        }

        private bool IsInWater(MultiTileInstance instance, WorldDataStore dataStore)
        {
            Vector2Int anchor = instance.Anchor;
            Vector2Int size = instance.TileSO?.Size ?? new Vector2Int(2, 2);

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int pos = new Vector2Int(anchor.x + i, anchor.y + j);
                    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int neighbor = pos + dir;
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

        private void TryDrainAttempt(MultiTileInstance instance, WorldDataStore dataStore)
        {
            // Debug.Log($"Attempting drain attempt from habitat core position {instance.Anchor}");

            Vector2Int anchor = instance.Anchor;
            Vector2Int size = instance.TileSO?.Size ?? new Vector2Int(2, 2);

            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    visited.Add(new Vector2Int(anchor.x + i, anchor.y + j));
                }
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
                        if (!visited.Contains(neighbor) && dataStore.IsWaterCell(neighbor.x, neighbor.y) && dataStore.IsPlayerPlacedBackgroundAt(neighbor.x, neighbor.y))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

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

                    if (!dataStore.IsInBounds(next.x, next.y))
                    {
                        OnDrainFailure();
                        return;
                    }

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

            OnDrainSuccess(visited, dataStore);
        }

        private void OnDrainSuccess(HashSet<Vector2Int> visited, WorldDataStore dataStore)
        {
            Debug.Log("Sponge Success: Enclosed area found within capacity.");
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

        public override void OnRemoved(MultiTileInstance instance, WorldDataStore dataStore)
        {
            if (instance == null || dataStore == null)
            {
                return;
            }

            Vector2Int anchor = instance.Anchor;
            Vector2Int size = instance.TileSO?.Size ?? new Vector2Int(2, 2);
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int pos = new Vector2Int(anchor.x + i, anchor.y + j);
                    Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int neighbor = pos + dir;
                        if (visited.Contains(neighbor))
                        {
                            continue;
                        }

                        if (dataStore.IsUnderwaterAirAt(neighbor.x, neighbor.y))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                dataStore.SetUnderwaterAir(current.x, current.y, false);

                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int next = current + dir;
                    if (!visited.Contains(next) && dataStore.IsUnderwaterAirAt(next.x, next.y) && dataStore.GetTileId(next.x, next.y, WorldTm.ForegroundTilemap) == GameDataRegistry.INVALID_ID)
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }
    }
}
