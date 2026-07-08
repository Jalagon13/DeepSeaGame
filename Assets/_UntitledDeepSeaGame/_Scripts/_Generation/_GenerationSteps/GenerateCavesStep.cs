using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class GenerateCavesStep : GenerationStep
    {
        [Header("Caves")]
        [SerializeField, Range(0f, 1f)]
        private float _fillProbability = 0.52f; // Chance that a cell starts as a wall during initial random fill. Higher = denser caves (more solid).

        [SerializeField, Range(0f, 1f)]
        private float _minFillProbability = 0.465f; // Lower bound for the noise-based local fill probability.

        [SerializeField, Range(0f, 1f)]
        private float _maxFillProbability = 0.525f; // Upper bound for the noise-based local fill probability.

        [SerializeField, Range(0.0001f, 0.05f)]
        private float _noiseScale = 0.008f; // Lower values make the noise blobs larger and smoother.

        [SerializeField, Range(0, 8)]
        private int _smoothingIterations = 3; // Number of smoothing passes. More iterations -> larger, more connected pockets; fewer -> many small pockets.

        public override WorldGenerationState State => WorldGenerationState.CarvingCaves;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            int columnsPerFrame = Mathf.Max(1, context.Config.ColumnsPerFrame);

            bool[,] grid = new bool[width, height];
            float[,] noiseMap = new float[width, height];

            // Build a low-frequency noise map so different regions get slightly different initial fill probabilities.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noiseMap[x, y] = Mathf.PerlinNoise(x * _noiseScale, y * _noiseScale);
                }
            }

            // Initial random fill below the surface heights using the seeded RNG from the context.
            for (int x = 0; x < width; x++)
            {
                int surface = context.SurfaceHeights[x];
                for (int y = 0; y < height; y++)
                {
                    // Cells above the sea level are treated as air for CA purposes.
                    if (y > surface)
                    {
                        grid[x, y] = false;
                    }
                    else if (y <= surface)
                    {
                        float localFillProbability = Mathf.Lerp(_minFillProbability, _maxFillProbability, noiseMap[x, y]);
                        localFillProbability = Mathf.Clamp01(localFillProbability);

                        // Below (or at) the sea level: random fill based on the local noise-driven probability (true = wall)
                        grid[x, y] = context.Random.NextDouble() < localFillProbability;
                        // Debug.Log($"localFillProbability = {localFillProbability:F3}, filled = {grid[x, y]}");
                    }
                }

                if ((x + 1) % columnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            // Smoothing passes using the 8-neighbour rule: count neighbors (>4 -> wall, <4 -> open, ==4 -> keep)
            for (int iter = 0; iter < _smoothingIterations; iter++)
            {
                bool[,] next = new bool[width, height];

                for (int x = 0; x < width; x++)
                {
                    // int surface = context.SurfaceHeights[x];
                    for (int y = 0; y < height; y++)
                    {
                        // Keep above-sea level cells as walls/solid and do not alter them.
                        if (y >= context.Config.SeaLevelY)
                        {
                            next[x, y] = true;
                            continue;
                        }

                        int wallCount = 0;
                        for (int nx = x - 1; nx <= x + 1; nx++)
                        {
                            for (int ny = y - 1; ny <= y + 1; ny++)
                            {
                                if (nx == x && ny == y) continue;

                                // Out-of-bounds counts as wall
                                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                {
                                    wallCount++;
                                    continue;
                                }

                                // Treat cells that are above their column surface as walls for neighbor counting
                                if (ny >= context.Config.SeaLevelY)
                                {
                                    wallCount++;
                                    continue;
                                }

                                if (grid[nx, ny]) wallCount++;
                            }
                        }

                        if (wallCount > 4) next[x, y] = true;
                        else if (wallCount < 4) next[x, y] = false;
                        else next[x, y] = grid[x, y];
                    }
                }

                grid = next;
                // yield between iterations so editor stays responsive on large worlds
                yield return null;
            }

            // Apply final grid to the foreground tilemap: wall -> solid tile, open -> empty (air)
            // We intentionally only modify the foreground here so this step controls cave shape only.
            for (int x = 0; x < width; x++)
            {
                // int surface = context.SurfaceHeights[x];
                for (int y = 0; y < height; y++)
                {
                    // Do not modify tiles above the sea level; they're considered regular ground and left alone.
                    if (y >= context.Config.SeaLevelY) continue;

                    // For solid cells we set the foreground to the world's configured solid tile id.
                    ushort tileId = grid[x, y] ? context.SolidTileId : GameDataRegistry.INVALID_ID;
                    context.DataStore.SetTileId(x, y, tileId, WorldTm.ForegroundTilemap);
                }

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
