using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class BuildSurfaceHeightmapStep : GenerationStep
    {
        [Header("Safe Shallows Surface")]
        [SerializeField] private float _baseHeight = 18f;
        [SerializeField, Range(0.001f, 0.1f)] private float _largeShapeNoiseScale = 0.03f;
        [SerializeField] private float _largeShapeAmplitude = 6f;
        [SerializeField, Range(0.01f, 0.5f)] private float _detailNoiseScale = 0.16f;
        [SerializeField] private float _detailAmplitude = 1.4f;

        public override WorldGenerationState State => WorldGenerationState.GeneratingSurface;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int maxHeight = Mathf.Max(1, context.Config.WorldHeight);
            int columnsPerFrame = Mathf.Max(1, context.Config.ColumnsPerFrame);

            // Seed the sampling so each world seed creates a distinct rolling seabed silhouette.
            float seedOffset = Mathf.Abs(context.SeedHash % 10000) * 0.01f + 0.5f;

            for (int x = 0; x < width; x++)
            {
                // The large-shape layer uses a low frequency and a larger amplitude to create
                // gentle, wide rolling hills that define the overall floor contour for the biome.
                float largeShapeSample = Mathf.PerlinNoise((x + seedOffset) * _largeShapeNoiseScale, 0f);
                float largeShapeOffset = (largeShapeSample * 2f - 1f) * _largeShapeAmplitude;

                // The detail layer adds a higher-frequency, lower-amplitude pass on top of the
                // broad hills so the seafloor feels a bit rougher and more natural without becoming jagged.
                float detailSample = Mathf.PerlinNoise((x + seedOffset * 2f + 17f) * _detailNoiseScale, 0f);
                float detailOffset = (detailSample * 2f - 1f) * _detailAmplitude;

                float combinedHeight = _baseHeight + largeShapeOffset + detailOffset;
                context.SurfaceHeights[x] = Mathf.RoundToInt(Mathf.Clamp(combinedHeight, 0f, maxHeight));

                if ((x + 1) % columnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            context.SetStepProgress(1f);
        }
    }
}
