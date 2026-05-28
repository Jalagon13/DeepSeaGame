using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Sponge Data", menuName = "Data/SpongeData")]
    public class SpongeItemSO : ItemSO
    {
        [field: SerializeField]
        public int MaxTileDetection { get; private set; } = 40;
        
        public void TryDrainAttempt()
        {
            Debug.Log($"Attempting drain attempt");
            
            Vector2Int startPos = GameManager.MouseTilePosition;
            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;

            if (dataStore == null) return;

            // 1. Initial check: Is the starting point in bounds and "water" (empty foreground)?
            // NTFS: this only checks if the forground tile is completely empty not if its a walk throughable forground tile or not
            if (!dataStore.IsInBounds(startPos.x, startPos.y) || dataStore.GetTileId(startPos.x, startPos.y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID || dataStore.IsAirAt(startPos.x, startPos.y))
            {
                return;
            }

            // 2. Initialize BFS flood fill
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();

            queue.Enqueue(startPos);
            visited.Add(startPos);

            Vector2Int[] neighbors = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};

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

                    // If the neighbor is empty space in the foreground, it's more "water" to explore
                    if (dataStore.GetTileId(next.x, next.y, WorldTm.ForegroundTilemap) == GameDataRegistry.INVALID_ID)
                    {
                        if (!visited.Contains(next))
                        {
                            visited.Add(next);

                            // If we exceed the sponge's capacity, it's a failure
                            if (visited.Count > MaxTileDetection)
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
                if (!dataStore.IsAirAt(pos.x, pos.y))
                {
                    dataStore.SetAirValue(pos.x, pos.y, true);
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