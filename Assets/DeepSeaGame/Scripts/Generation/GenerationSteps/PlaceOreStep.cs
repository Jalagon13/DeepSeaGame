using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    public class PlaceOreStep : GenerationStep
    {
        [Serializable]
        private class OreDefinition
        {
            [SerializeField] private TileSO _oreTile;
            [SerializeField, Min(0f), Tooltip("Relative frequency of ore vein placement. Lower values make this ore rarer.")]
            private float _spawnFrequency = 1f;
            [SerializeField, Min(1f), Tooltip("Minimum spacing between ore vein seeds. Larger values reduce ore density.")]
            private float _seedSpacing = 8f;
            [SerializeField, Min(0), Tooltip("Minimum depth at which this ore can spawn.")]
            private int _minDepth = 6;
            [SerializeField, Min(0), Tooltip("Maximum depth at which this ore can spawn.")]
            private int _maxDepth = 70;
            [SerializeField, Min(1), Tooltip("Minimum number of tiles in a vein.")]
            private int _minVeinSize = 3;
            [SerializeField, Min(1), Tooltip("Maximum number of tiles in a vein.")]
            private int _maxVeinSize = 8;
            [Range(0f, 1f), Tooltip("0 = straight vein, 1 = blob-like vein.")]
            [SerializeField] private float _veinRoundness = 0.35f;
            [SerializeField, Tooltip("Optional host material required for this ore to replace.")]
            private TileSO _requiredHostMaterial;

            public TileSO OreTile => _oreTile;
            public float SpawnFrequency => _spawnFrequency;
            public float SeedSpacing => _seedSpacing;
            public int MinDepth => _minDepth;
            public int MaxDepth => _maxDepth;
            public int MinVeinSize => _minVeinSize;
            public int MaxVeinSize => _maxVeinSize;
            public float VeinRoundness => _veinRoundness;
            public TileSO RequiredHostMaterial => _requiredHostMaterial;
        }

        [Header("Ore")]
        [SerializeField] private List<OreDefinition> _oreDefinitions = new List<OreDefinition>();
        [SerializeField, Min(1)] private int _maxCandidateRejectionsPerPoint = 32;
        [SerializeField, Min(1)] private int _maxSeedPlacementAttempts = 80;

        public override WorldGenerationState State => WorldGenerationState.PlacingIronOre;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            if (GameDataRegistry.Instance == null)
            {
                Debug.LogWarning("GameDataRegistry is unavailable while placing ore veins.");
                yield break;
            }

            // This step runs after the terrain has already been carved and filled, so it can place ore into
            // the existing host material instead of creating blocks from scratch.
            System.Random random = context.Random;

            for (int i = 0; i < _oreDefinitions.Count; i++)
            {
                OreDefinition definition = _oreDefinitions[i];
                if (definition == null || definition.OreTile == null)
                {
                    continue;
                }

                PlaceOreDefinition(context, definition, random);
                context.SetStepProgress((i + 1f) / Mathf.Max(1, _oreDefinitions.Count));
                yield return null;
            }

            context.SetStepProgress(1f);
        }

        private void PlaceOreDefinition(WorldGenerationContext context, OreDefinition definition, System.Random random)
        {
            ushort oreTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(definition.OreTile);
            if (oreTileId == GameDataRegistry.INVALID_ID)
            {
                return;
            }

            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            float cellSize = Mathf.Max(0.5f, definition.SeedSpacing / Mathf.Sqrt(2f));
            int gridWidth = Mathf.Max(1, Mathf.CeilToInt(width / cellSize));
            int gridHeight = Mathf.Max(1, Mathf.CeilToInt(height / cellSize));
            Vector2Int?[,] seedGrid = new Vector2Int?[gridWidth, gridHeight];
            List<Vector2Int> activePoints = new List<Vector2Int>();
            List<Vector2Int> acceptedSeeds = new List<Vector2Int>();

            // The active list is the standard Poisson-disk workflow: once a seed is accepted, it can still
            // generate new candidate points around itself until it no longer finds suitable spacing.
            int initialSeedCount = TryGetInitialSeeds(context, definition, random, width, height, seedGrid, cellSize, activePoints, acceptedSeeds);
            if (initialSeedCount > 0)
            {
                int activeIndex = 0;
                while (activeIndex < activePoints.Count)
                {
                    Vector2Int center = activePoints[activeIndex];
                    bool accepted = false;

                    // Each candidate is tested against spacing and host-tile validity before it can become a new seed.
                    // If it keeps failing, we stop trying that branch so the generator does not loop forever in sparse regions.
                    for (int attempt = 0; attempt < _maxCandidateRejectionsPerPoint; attempt++)
                    {
                        Vector2Int candidate = GetCandidateAroundPoint(center, definition.SeedSpacing, random, width, height);
                        if (!IsWithinDepthBand(candidate.y, definition) || !IsValidHostTile(context, candidate.x, candidate.y, definition))
                        {
                            continue;
                        }

                        if (IsTooCloseToExistingSeed(candidate, seedGrid, cellSize, definition.SeedSpacing))
                        {
                            continue;
                        }

                        RegisterSeed(candidate, seedGrid, cellSize, activePoints, acceptedSeeds);
                        accepted = true;
                        break;
                    }

                    if (!accepted)
                    {
                        activePoints.RemoveAt(activeIndex);
                    }
                    else
                    {
                        activeIndex++;
                    }
                }
            }

            foreach (Vector2Int seed in acceptedSeeds)
            {
                GrowVein(context, definition, oreTileId, seed, random);
            }
        }

        private int TryGetInitialSeeds(WorldGenerationContext context, OreDefinition definition, System.Random random, int width, int height,
            Vector2Int?[,] seedGrid, float cellSize, List<Vector2Int> activePoints, List<Vector2Int> acceptedSeeds)
        {
            int targetSeedCount = definition.SpawnFrequency <= 0f
                ? 0
                : Mathf.Max(1, Mathf.CeilToInt((width * height) * definition.SpawnFrequency / Mathf.Max(1f, definition.SeedSpacing * definition.SeedSpacing * 32f)));
            int placedSeeds = 0;

            for (int attempt = 0; attempt < _maxSeedPlacementAttempts && placedSeeds < targetSeedCount; attempt++)
            {
                Vector2Int candidate = GetRandomPoint(width, height, random);
                if (!IsWithinDepthBand(candidate.y, definition) || !IsValidHostTile(context, candidate.x, candidate.y, definition))
                {
                    continue;
                }

                if (IsTooCloseToExistingSeed(candidate, seedGrid, cellSize, definition.SeedSpacing))
                {
                    continue;
                }

                RegisterSeed(candidate, seedGrid, cellSize, activePoints, acceptedSeeds);
                placedSeeds++;
            }

            return placedSeeds;
        }

        private void RegisterSeed(Vector2Int point, Vector2Int?[,] seedGrid, float cellSize, List<Vector2Int> activePoints, List<Vector2Int> acceptedSeeds)
        {
            int gridX = Mathf.Clamp(Mathf.FloorToInt(point.x / cellSize), 0, seedGrid.GetLength(0) - 1);
            int gridY = Mathf.Clamp(Mathf.FloorToInt(point.y / cellSize), 0, seedGrid.GetLength(1) - 1);
            seedGrid[gridX, gridY] = point;
            activePoints.Add(point);
            acceptedSeeds.Add(point);
        }

        private Vector2Int GetRandomPoint(int width, int height, System.Random random)
        {
            return new Vector2Int(random.Next(width), random.Next(height));
        }

        private Vector2Int GetCandidateAroundPoint(Vector2Int center, float minSeedDistance, System.Random random, int width, int height)
        {
            float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
            float radius = minSeedDistance + (float)(random.NextDouble() * minSeedDistance);
            int x = center.x + Mathf.RoundToInt(Mathf.Cos(angle) * radius);
            int y = center.y + Mathf.RoundToInt(Mathf.Sin(angle) * radius);
            return new Vector2Int(Mathf.Clamp(x, 0, width - 1), Mathf.Clamp(y, 0, height - 1));
        }

        private bool IsTooCloseToExistingSeed(Vector2Int candidate, Vector2Int?[,] seedGrid, float cellSize, float minSeedDistance)
        {
            int gridX = Mathf.Clamp(Mathf.FloorToInt(candidate.x / cellSize), 0, seedGrid.GetLength(0) - 1);
            int gridY = Mathf.Clamp(Mathf.FloorToInt(candidate.y / cellSize), 0, seedGrid.GetLength(1) - 1);
            int searchRadius = Mathf.CeilToInt(minSeedDistance / cellSize);

            for (int x = gridX - searchRadius; x <= gridX + searchRadius; x++)
            {
                for (int y = gridY - searchRadius; y <= gridY + searchRadius; y++)
                {
                    if (x < 0 || x >= seedGrid.GetLength(0) || y < 0 || y >= seedGrid.GetLength(1))
                    {
                        continue;
                    }

                    Vector2Int? existingPoint = seedGrid[x, y];
                    if (!existingPoint.HasValue)
                    {
                        continue;
                    }

                    Vector2Int delta = candidate - existingPoint.Value;
                    float distanceSquared = delta.x * delta.x + delta.y * delta.y;
                    if (distanceSquared < minSeedDistance * minSeedDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidHostTile(WorldGenerationContext context, int x, int y, OreDefinition definition)
        {
            if (!context.DataStore.IsInBounds(x, y))
            {
                return false;
            }

            if (!IsWithinDepthBand(y, definition))
            {
                return false;
            }

            ushort tileId = context.DataStore.GetTileId(x, y, WorldTm.ForegroundTilemap);
            if (tileId == GameDataRegistry.INVALID_ID)
            {
                return false;
            }

            if (GameDataRegistry.Instance == null)
            {
                return false;
            }

            TileSO currentTile = GameDataRegistry.Instance.GetTileSOFromTileId(tileId);
            if (currentTile == null)
            {
                return false;
            }

            if (definition.RequiredHostMaterial != null && currentTile != definition.RequiredHostMaterial)
            {
                return false;
            }

            return true;
        }

        private bool IsWithinDepthBand(int y, OreDefinition definition)
        {
            int minDepth = Mathf.Min(definition.MinDepth, definition.MaxDepth);
            int maxDepth = Mathf.Max(definition.MinDepth, definition.MaxDepth);
            return y >= minDepth && y <= maxDepth;
        }

        private void GrowVein(WorldGenerationContext context, OreDefinition definition, ushort oreTileId, Vector2Int seed, System.Random random)
        {
            int targetSize = random.Next(definition.MinVeinSize, definition.MaxVeinSize + 1);
            HashSet<Vector2Int> veinCells = new HashSet<Vector2Int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            if (!TryPlaceOreTile(context, definition, oreTileId, seed, veinCells))
            {
                return;
            }

            frontier.Enqueue(seed);

            while (frontier.Count > 0 && veinCells.Count < targetSize)
            {
                Vector2Int current = frontier.Dequeue();
                Vector2Int next = GetNextVeinStep(context, definition, oreTileId, current, random, veinCells);
                if (next == Vector2Int.zero)
                {
                    continue;
                }

                if (TryPlaceOreTile(context, definition, oreTileId, next, veinCells))
                {
                    frontier.Enqueue(next);
                }
            }
        }

        private Vector2Int GetNextVeinStep(WorldGenerationContext context, OreDefinition definition, ushort oreTileId, Vector2Int current,
            System.Random random, HashSet<Vector2Int> veinCells)
        {
            List<Vector2Int> directions = new List<Vector2Int>
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down,
                new(1, 1),
                new(1, -1),
                new(-1, 1),
                new(-1, -1)
            };

            // Lower roundness favors a straighter, snaking vein; higher roundness opens up more side branches and blob-like growth.
            if (random.NextDouble() < Mathf.Lerp(0.75f, 0.35f, definition.VeinRoundness))
            {
                directions.Reverse();
            }

            for (int i = 0; i < directions.Count; i++)
            {
                Vector2Int candidate = current + directions[i];
                if (!IsValidHostTile(context, candidate.x, candidate.y, definition))
                {
                    continue;
                }

                if (veinCells.Contains(candidate))
                {
                    continue;
                }

                ushort currentTileId = context.DataStore.GetTileId(candidate.x, candidate.y, WorldTm.ForegroundTilemap);
                if (currentTileId == oreTileId)
                {
                    continue;
                }

                return candidate;
            }

            return Vector2Int.zero;
        }

        private bool TryPlaceOreTile(WorldGenerationContext context, OreDefinition definition, ushort oreTileId, Vector2Int position, HashSet<Vector2Int> veinCells)
        {
            if (!IsValidHostTile(context, position.x, position.y, definition))
            {
                return false;
            }

            if (veinCells.Contains(position))
            {
                return false;
            }

            context.DataStore.SetForegroundTileId(position.x, position.y, oreTileId);
            veinCells.Add(position);
            return true;
        }
    }
}
