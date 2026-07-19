using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "Shelter Core Behavior", menuName = "MultiTile/Lifecycle/ShelterCore")]
    public class ShelterCoreBehavior : MultiTileBehavior
    {
        [SerializeField] private int _maxTileDetection = 40;
        [SerializeField] private float _drainInterval = 5f;
        [SerializeField] private int _minYHeightToWork = 250;

        public override void Update(MultiTileInstance instance, WorldDataStore dataStore, float deltaTime)
        {
            if(!dataStore.IsInWater(instance.Anchor.x, instance.Anchor.y) || instance.Anchor.y < _minYHeightToWork)
            {
                return;
            }
            
            instance.Timer += deltaTime;
            if (instance.Timer >= _drainInterval)
            {
                instance.Timer -= _drainInterval;
                OnTimerComplete(instance, dataStore);
            }
        }

        private void OnTimerComplete(MultiTileInstance instance, WorldDataStore dataStore)
        {
            if (IsSpaceClosedOff(instance.Anchor, dataStore, out HashSet<Vector2Int> visited))
            {
                DrainWater(instance.Anchor, dataStore, visited);
            }
        }

        // Flood fills to check if the space is closed off by foreground tiles and its size is less than or equal to _maxTileDetection
        private bool IsSpaceClosedOff(Vector2Int startPos, WorldDataStore dataStore, out HashSet<Vector2Int> visited)
        {
            Queue<Vector2Int> queue = new();
            visited = new();
            
            queue.Enqueue(startPos);
            visited.Add(startPos);

            Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            while (queue.Count > 0)
            {
                // If the pocket is larger than our max detection size, it is not considered closed off (or too big)
                if (visited.Count > _maxTileDetection)
                {
                    return false;
                }

                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    // If we reach out of bounds, the space is open to the edge of the world
                    if (!dataStore.IsInBounds(neighbor.x, neighbor.y))
                    {
                        return false;
                    }

                    // If there is a valid solid foreground tile, it acts as a boundary wall
                    ushort tileId = dataStore.GetTileId(neighbor.x, neighbor.y);
                    if (tileId != GameDataRegistry.INVALID_ID && GameDataRegistry.Instance.GetTileSOFromTileId(tileId).IsSolid)
                    {
                        continue;
                    }

                    // If it is empty space and we haven't visited it yet, keep filling
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // If we finish checking and never exceeded max tiles or hit bounds, it is completely enclosed
            return true;
        }

        private void DrainWater(Vector2Int anchor, WorldDataStore dataStore, HashSet<Vector2Int> visited)
        {
            foreach (Vector2Int pos in visited)
            {
                dataStore.AddUnderwaterAirTile(pos.x, pos.y);
            }
            Debug.Log($"ShelterCore valid space detected at {anchor}. Drained {visited.Count} tiles.");
        }
    }
}
