using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HabitatCore : MonoBehaviour
    {
        [field: SerializeField]
        public int MaxTileDetection { get; private set; } = 40;

        private void TryDrainAttempt()
        {
            Debug.Log($"Attempting drain attempt");

            Vector2Int startPos = GameManager.MouseTilePosition;
            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;

            if (dataStore == null) return;

            // 1. Initial check: sponge can only drain actual water cells in the ocean zone
            // and only if they have a player-placed wall behind them.
            if (!dataStore.IsWaterCell(startPos.x, startPos.y) || !dataStore.IsPlayerPlacedBackgroundAt(startPos.x, startPos.y))
            {
                return;
            }

            // 2. Initialize BFS flood fill
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();

            queue.Enqueue(startPos);
            visited.Add(startPos);

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
