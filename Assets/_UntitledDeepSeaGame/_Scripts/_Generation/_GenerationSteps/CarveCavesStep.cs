using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class CarveCavesStep : GenerationStep
    {
        [Header("Cave Bounds")]
        [SerializeField] private int _minimumCaveY = 8;
        [SerializeField] private int _surfaceClearanceForCaves = 8;
        [SerializeField] private int _walkerMinStartBelowSurface = 20;

        [Header("Walker Count")]
        [SerializeField] private int _caveWalkerCount = 18;
        [SerializeField] private int _caveWalkerSteps = 140;

        [Header("Walker Shape")]
        [SerializeField] private int _minimumCaveRadius = 2;
        [SerializeField] private int _maximumCaveRadius = 4;
        [SerializeField] private float _verticalDriftStrength = 0.8f;
        [SerializeField] private float _minimumHorizontalStep = 0.8f;
        [SerializeField] private float _maximumHorizontalStep = 1.4f;
        [SerializeField, Range(0f, 1f)] private float _horizontalFlipChance = 0.14f;
        [SerializeField, Range(0f, 1f)] private float _cavernPocketChance = 0.18f;

        public override WorldGenerationState State => WorldGenerationState.CarvingCaves;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int minimumCaveY = Mathf.Max(0, _minimumCaveY);
            int surfaceClearanceForCaves = Mathf.Max(1, _surfaceClearanceForCaves);
            int walkerMinStartBelowSurface = Mathf.Max(surfaceClearanceForCaves + 1, _walkerMinStartBelowSurface);
            int caveWalkerCount = Mathf.Max(1, _caveWalkerCount);
            int caveWalkerSteps = Mathf.Max(1, _caveWalkerSteps);
            int minimumCaveRadius = Mathf.Max(1, _minimumCaveRadius);
            int maximumCaveRadius = Mathf.Max(minimumCaveRadius, _maximumCaveRadius);
            float verticalDriftStrength = Mathf.Max(0.05f, _verticalDriftStrength);
            float minimumHorizontalStep = Mathf.Max(0.1f, _minimumHorizontalStep);
            float maximumHorizontalStep = Mathf.Max(minimumHorizontalStep, _maximumHorizontalStep);

            for (int walkerIndex = 0; walkerIndex < caveWalkerCount; walkerIndex++)
            {
                int startX = context.Random.Next(4, context.Config.WorldWidth - 4);
                int maxStartY = Mathf.Max(minimumCaveY + 1, context.SurfaceHeights[startX] - walkerMinStartBelowSurface);
                int startY = context.Random.Next(minimumCaveY, Mathf.Max(minimumCaveY + 1, maxStartY + 1));

                float positionX = startX;
                float positionY = startY;
                float horizontalDirection = context.Random.NextDouble() < 0.5 ? -1f : 1f;

                for (int step = 0; step < caveWalkerSteps; step++)
                {
                    if (context.Random.NextDouble() < _horizontalFlipChance)
                    {
                        horizontalDirection *= -1f;
                    }

                    float horizontalStep = Mathf.Lerp(minimumHorizontalStep, maximumHorizontalStep, (float)context.Random.NextDouble());
                    float verticalStep = Mathf.Lerp(-verticalDriftStrength, verticalDriftStrength, (float)context.Random.NextDouble());

                    positionX += horizontalDirection * horizontalStep;
                    positionY += verticalStep;

                    int currentX = Mathf.Clamp(Mathf.RoundToInt(positionX), 4, context.Config.WorldWidth - 5);
                    int surfaceLimit = context.SurfaceHeights[currentX] - surfaceClearanceForCaves;
                    positionY = Mathf.Clamp(positionY, minimumCaveY, surfaceLimit);

                    int radius = context.Random.Next(minimumCaveRadius, maximumCaveRadius + 1);
                    if (context.Random.NextDouble() < _cavernPocketChance)
                    {
                        radius += 1;
                    }

                    CarveCircle(context, currentX, Mathf.RoundToInt(positionY), radius, minimumCaveY, surfaceClearanceForCaves);

                    if ((step + 1) % context.Config.ColumnsPerFrame == 0)
                    {
                        float overallWalkerProgress = (walkerIndex + ((step + 1f) / caveWalkerSteps)) / caveWalkerCount;
                        context.SetStepProgress(overallWalkerProgress);
                        yield return null;
                    }
                }
            }

            context.SetStepProgress(1f);
        }

        private void CarveCircle(WorldGenerationContext context, int centerX, int centerY, int radius, int minimumCaveY, int surfaceClearanceForCaves)
        {
            int radiusSquared = radius * radius;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x < 0 || x >= context.Config.WorldWidth)
                {
                    continue;
                }

                int surfaceLimit = context.SurfaceHeights[x] - surfaceClearanceForCaves;

                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (y < minimumCaveY || y >= context.Config.WorldHeight)
                    {
                        continue;
                    }

                    if (y > surfaceLimit)
                    {
                        continue;
                    }

                    int deltaX = x - centerX;
                    int deltaY = y - centerY;
                    if ((deltaX * deltaX) + (deltaY * deltaY) > radiusSquared)
                    {
                        continue;
                    }

                    context.DataStore.SetTileId(x, y, GameDataRegistry.INVALID_ID);
                }
            }
        }
    }
}
