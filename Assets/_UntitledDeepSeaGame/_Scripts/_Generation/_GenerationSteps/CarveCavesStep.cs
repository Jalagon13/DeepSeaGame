using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class CarveCavesStep : GenerationStep
    {
        [Header("Cave Bounds")]
        [SerializeField] private int _minimumCaveY = 8;
        [SerializeField] private int _surfaceClearanceForCaves = 8;
        [SerializeField] private int _noiseCaveStartBelowSurface = 16;

        [Header("Perlin Noise")]
        [SerializeField] private float _caveNoiseScale = 36f;
        [SerializeField] private float _verticalStretch = 0.75f;
        [SerializeField] private int _octaves = 4;
        [SerializeField, Range(0f, 1f)] private float _persistence = 0.5f;
        [SerializeField] private float _lacunarity = 2f;
        [SerializeField, Range(0f, 1f)] private float _caveThreshold = 0.68f;

        public override WorldGenerationState State => WorldGenerationState.CarvingCaves;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            int minimumCaveY = Mathf.Max(0, _minimumCaveY);
            int surfaceClearanceForCaves = Mathf.Max(1, _surfaceClearanceForCaves);
            int startBelowSurface = Mathf.Max(surfaceClearanceForCaves + 1, _noiseCaveStartBelowSurface);
            float caveNoiseScale = Mathf.Max(1f, _caveNoiseScale);
            float verticalStretch = Mathf.Max(0.05f, _verticalStretch);
            int octaves = Mathf.Max(1, _octaves);
            float persistence = Mathf.Clamp01(_persistence);
            float lacunarity = Mathf.Max(1f, _lacunarity);

            float offsetX = Mathf.Abs((context.SeedHash * 17) % 10000) + 13.371f;
            float offsetY = Mathf.Abs((context.SeedHash * 43) % 10000) + 41.913f;

            for (int x = 0; x < width; x++)
            {
                int maxCarveY = Mathf.Min(height - 1, context.SurfaceHeights[x] - startBelowSurface);
                if (maxCarveY < minimumCaveY)
                {
                    continue;
                }

                for (int y = minimumCaveY; y <= maxCarveY; y++)
                {
                    float sample = SampleFractalNoise(x, y, offsetX, offsetY, caveNoiseScale, verticalStretch, octaves, persistence, lacunarity);
                    if (sample >= _caveThreshold)
                    {
                        context.DataStore.SetTileId(x, y, GameDataRegistry.INVALID_ID);
                    }
                }

                if ((x + 1) % context.Config.ColumnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            context.SetStepProgress(1f);
        }

        private float SampleFractalNoise(int x, int y, float offsetX, float offsetY, float baseScale, float verticalStretch, int octaves, float persistence, float lacunarity)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float total = 0f;
            float amplitudeSum = 0f;

            for (int octave = 0; octave < octaves; octave++)
            {
                float sampleX = ((x + offsetX) / baseScale) * frequency;
                float sampleY = (((y * verticalStretch) + offsetY) / baseScale) * frequency;
                float noise = Mathf.PerlinNoise(sampleX, sampleY);

                total += noise * amplitude;
                amplitudeSum += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            if (amplitudeSum <= 0f)
            {
                return 0f;
            }

            return total / amplitudeSum;
        }
    }
}
