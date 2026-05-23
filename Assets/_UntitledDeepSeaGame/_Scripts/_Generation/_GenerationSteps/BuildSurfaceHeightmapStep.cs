using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class BuildSurfaceHeightmapStep : GenerationStep
    {
        [Header("Surface Shape")]
        [SerializeField, Range(0f, 1f)] private float _surfaceBaseHeightPercent = 0.6f;
        [SerializeField] private int _surfaceAmplitude = 18;
        [SerializeField] private float _primarySurfaceNoiseScale = 72f;
        [SerializeField] private float _secondarySurfaceNoiseScale = 28f;
        [SerializeField, Range(0f, 1f)] private float _secondarySurfaceNoiseStrength = 0.35f;
        [SerializeField] private bool _applyThreePointSmoothing = true;

        public override WorldGenerationState State => WorldGenerationState.GeneratingSurface;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int baseSurfaceHeight = Mathf.RoundToInt(context.Config.WorldHeight * _surfaceBaseHeightPercent);
            int surfaceAmplitude = Mathf.Max(1, _surfaceAmplitude);
            int minHeight = baseSurfaceHeight - surfaceAmplitude;
            int maxHeight = baseSurfaceHeight + surfaceAmplitude;
            float primaryScale = Mathf.Max(1f, _primarySurfaceNoiseScale);
            float secondaryScale = Mathf.Max(1f, _secondarySurfaceNoiseScale);
            float primaryOffset = Mathf.Abs(context.SeedHash % 10000) + 0.137f;
            float secondaryOffset = Mathf.Abs((context.SeedHash * 31) % 10000) + 91.713f;

            for (int x = 0; x < width; x++)
            {
                float primarySample = Mathf.PerlinNoise((x + primaryOffset) / primaryScale, primaryOffset);
                float secondarySample = Mathf.PerlinNoise((x + secondaryOffset) / secondaryScale, secondaryOffset);
                float combinedSample = Mathf.Lerp(primarySample, secondarySample, _secondarySurfaceNoiseStrength);
                int surfaceHeight = Mathf.RoundToInt(Mathf.Lerp(minHeight, maxHeight, combinedSample));
                context.SurfaceHeights[x] = Mathf.Clamp(surfaceHeight, minHeight, maxHeight);

                if ((x + 1) % context.Config.ColumnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            if (!_applyThreePointSmoothing)
            {
                context.SetStepProgress(1f);
                yield break;
            }

            int[] smoothedHeights = new int[width];
            for (int x = 0; x < width; x++)
            {
                int left = context.SurfaceHeights[Mathf.Max(0, x - 1)];
                int center = context.SurfaceHeights[x];
                int right = context.SurfaceHeights[Mathf.Min(width - 1, x + 1)];
                smoothedHeights[x] = Mathf.RoundToInt((left + center + right) / 3f);

                if ((x + 1) % context.Config.ColumnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                context.SurfaceHeights[x] = smoothedHeights[x];
            }

            context.SetStepProgress(1f);
        }
    }
}
