using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class CarveCaveEntrancesStep : GenerationStep
    {
        [Header("Spacing")]
        [SerializeField] private int _minDistanceBetweenEntrances = 60;
        [SerializeField] private int _maxDistanceBetweenEntrances = 120;

        [Header("Shaft Shape")]
        [SerializeField] private int _minEntranceWidth = 3;
        [SerializeField] private int _maxEntranceWidth = 6;
        [SerializeField] private float _horizontalJitter = 0.6f;

        [Header("Depth")]
        [Tooltip("The vertical distance to carve down from the ocean floor. Should match or exceed 'Noise Cave Start Below Surface' in CarveCavesStep (default 16).")]
        [SerializeField] private int _depthToReachBelowSurface = 22;

        public override WorldGenerationState State => WorldGenerationState.CarvingCaveEntrances;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int worldWidth = context.Config.WorldWidth;
            
            // Start at a random offset to avoid always having an entrance at the far left
            int currentX = context.Random.Next(_minDistanceBetweenEntrances / 2, _maxDistanceBetweenEntrances);

            while (currentX < worldWidth)
            {
                CarveEntranceShaft(context, currentX);

                // Determine distance to the next entrance
                int nextSpacing = context.Random.Next(_minDistanceBetweenEntrances, _maxDistanceBetweenEntrances + 1);
                currentX += nextSpacing;

                context.SetStepProgress((float)currentX / worldWidth);
                yield return null;
            }

            context.SetStepProgress(1f);
        }

        private void CarveEntranceShaft(WorldGenerationContext context, int startX)
        {
            // Ensure we are in bounds for the height lookup
            if (startX < 0 || startX >= context.Config.WorldWidth) return;

            int startY = context.SurfaceHeights[startX];
            float currentX = startX;

            for (int yOffset = 0; yOffset < _depthToReachBelowSurface; yOffset++)
            {
                int y = startY - yOffset;
                if (y < 0) break;

                // Determine radius for this specific depth layer
                float radius = context.Random.Next(_minEntranceWidth, _maxEntranceWidth + 1) * 0.5f;
                int xMin = Mathf.FloorToInt(currentX - radius);
                int xMax = Mathf.CeilToInt(currentX + radius);

                for (int x = xMin; x <= xMax; x++)
                {
                    // Carve if within the wobbly radius
                    if (Mathf.Abs(x - currentX) <= radius)
                    {
                        if (context.DataStore.IsInBounds(x, y))
                        {
                            context.DataStore.SetTileId(x, y, GameDataRegistry.INVALID_ID);
                        }
                    }
                }

                // Slowly drift the center of the shaft horizontally as we go deeper
                currentX += (float)(context.Random.NextDouble() * 2.0 - 1.0) * _horizontalJitter;
            }
        }
    }
}
