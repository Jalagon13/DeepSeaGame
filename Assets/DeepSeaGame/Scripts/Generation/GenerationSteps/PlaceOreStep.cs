using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    [Serializable]
    public struct OreSetting
    {
        public TileSO OreTile;
        public int MinDepth;
        public int MaxDepth;
        public int ClumpsPerChunk;
        public int MinClumpSize;
        public int MaxClumpSize;
    }

    public class PlaceOreStep : GenerationStep
    {
        [Header("Ore Generation")]
        [SerializeField] private TileSO _stoneTileSO;
        [SerializeField, Min(1)] private int _chunkSize = 64;
        [SerializeField] private List<OreSetting> _oreSettings = new List<OreSetting>();

        private static readonly Vector2Int[] AdjacentOffsets = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public override WorldGenerationState State => WorldGenerationState.PlacingIronOre;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            if (context == null)
            {
                yield break;
            }

            if (_stoneTileSO == null)
            {
                Debug.LogWarning("PlaceOreStep requires a Stone TileSO to identify stone blocks.");
                yield break;
            }

            if (_oreSettings == null || _oreSettings.Count == 0)
            {
                yield break;
            }

            int worldWidth = context.Config.WorldWidth;
            int worldHeight = context.Config.WorldHeight;
            int chunkSize = Mathf.Max(1, _chunkSize);
            WorldDataStore dataStore = context.DataStore;
            System.Random random = context.Random ?? new System.Random(context.SeedHash);
            ushort stoneTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_stoneTileSO);
            if (stoneTileId == GameDataRegistry.INVALID_ID)
            {
                Debug.LogWarning("PlaceOreStep could not resolve the stone tile ID.");
                yield break;
            }

            for (int chunkY = 0; chunkY < worldHeight; chunkY += chunkSize)
            {
                int currentChunkHeight = Mathf.Min(chunkSize, worldHeight - chunkY);
                bool chunkHasPotentialOre = false;

                for (int oreIndex = 0; oreIndex < _oreSettings.Count; oreIndex++)
                {
                    OreSetting oreSetting = _oreSettings[oreIndex];
                    if (oreSetting.OreTile == null)
                    {
                        continue;
                    }

                    int oreMinDepth = Mathf.Min(oreSetting.MinDepth, oreSetting.MaxDepth);
                    int oreMaxDepth = Mathf.Max(oreSetting.MinDepth, oreSetting.MaxDepth);
                    int chunkMinDepth = chunkY;
                    int chunkMaxDepth = chunkY + currentChunkHeight - 1;

                    if (oreMaxDepth < chunkMinDepth || oreMinDepth > chunkMaxDepth)
                    {
                        continue;
                    }

                    chunkHasPotentialOre = true;
                    break;
                }

                if (!chunkHasPotentialOre)
                {
                    context.SetStepProgress((chunkY + currentChunkHeight) / (float)worldHeight);
                    yield return null;
                    continue;
                }

                for (int chunkX = 0; chunkX < worldWidth; chunkX += chunkSize)
                {
                    int currentChunkWidth = Mathf.Min(chunkSize, worldWidth - chunkX);
                    GenerateOresInChunk(dataStore, chunkX, chunkY, currentChunkWidth, currentChunkHeight, stoneTileId, random);
                }

                context.SetStepProgress((chunkY + currentChunkHeight) / (float)worldHeight);
                yield return null;
            }

            context.SetStepProgress(1f);
        }

        public void GenerateOresInChunk(WorldDataStore dataStore, int chunkGlobalX, int chunkGlobalY, int width, int height, ushort stoneTileId, System.Random random = null)
        {
            if (dataStore == null || width <= 0 || height <= 0)
            {
                return;
            }

            random ??= new System.Random();

            for (int oreIndex = 0; oreIndex < _oreSettings.Count; oreIndex++)
            {
                OreSetting oreSetting = _oreSettings[oreIndex];
                if (oreSetting.OreTile == null)
                {
                    continue;
                }

                int clumpsPerChunk = Mathf.Max(0, oreSetting.ClumpsPerChunk);
                int minClumpSize = Mathf.Max(1, oreSetting.MinClumpSize);
                int maxClumpSize = Mathf.Max(minClumpSize, oreSetting.MaxClumpSize);
                int minDepth = Mathf.Min(oreSetting.MinDepth, oreSetting.MaxDepth);
                int maxDepth = Mathf.Max(oreSetting.MinDepth, oreSetting.MaxDepth);
                ushort oreTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(oreSetting.OreTile);

                if (oreTileId == GameDataRegistry.INVALID_ID || clumpsPerChunk <= 0)
                {
                    continue;
                }

                int cols = Mathf.CeilToInt(Mathf.Sqrt(clumpsPerChunk));
                int rows = Mathf.CeilToInt(clumpsPerChunk / (float)cols);

                for (int clumpIndex = 0; clumpIndex < clumpsPerChunk; clumpIndex++)
                {
                    int cellX = clumpIndex % cols;
                    int cellY = clumpIndex / cols;

                    int regionStartX = Mathf.FloorToInt(cellX * width / (float)cols);
                    int regionEndX = Mathf.FloorToInt((cellX + 1) * width / (float)cols);
                    int regionStartY = Mathf.FloorToInt(cellY * height / (float)rows);
                    int regionEndY = Mathf.FloorToInt((cellY + 1) * height / (float)rows);

                    regionEndX = Mathf.Clamp(regionEndX, regionStartX + 1, width);
                    regionEndY = Mathf.Clamp(regionEndY, regionStartY + 1, height);

                    if (!TryFindClumpStart(dataStore, chunkGlobalX, chunkGlobalY, width, height, regionStartX, regionStartY, regionEndX - regionStartX, regionEndY - regionStartY, minDepth, maxDepth, stoneTileId, random, out int localX, out int localY))
                    {
                        continue;
                    }

                    int worldX = chunkGlobalX + localX;
                    int worldY = chunkGlobalY + localY;
                    int targetClumpSize = random.Next(minClumpSize, maxClumpSize + 1);
                    GrowOreClump(worldX, worldY, oreTileId, targetClumpSize, stoneTileId, dataStore, random);
                }
            }
        }

        private bool TryFindClumpStart(WorldDataStore dataStore, int chunkGlobalX, int chunkGlobalY, int chunkWidth, int chunkHeight, int regionStartX, int regionStartY, int regionWidth, int regionHeight, int minDepth, int maxDepth, ushort stoneTileId, System.Random random, out int localX, out int localY)
        {
            localX = 0;
            localY = 0;
            int attempts = 6;

            for (int i = 0; i < attempts; i++)
            {
                int candidateX = regionStartX + random.Next(0, regionWidth);
                int candidateY = regionStartY + random.Next(0, regionHeight);
                int worldY = chunkGlobalY + candidateY;

                if (worldY < minDepth || worldY > maxDepth)
                {
                    continue;
                }

                int worldX = chunkGlobalX + candidateX;
                if (!dataStore.IsInBounds(worldX, worldY))
                {
                    continue;
                }

                if (dataStore.GetTileId(worldX, worldY, WorldTm.ForegroundTilemap) != stoneTileId)
                {
                    continue;
                }

                localX = candidateX;
                localY = candidateY;
                return true;
            }

            for (int x = regionStartX; x < regionStartX + regionWidth; x++)
            {
                for (int y = regionStartY; y < regionStartY + regionHeight; y++)
                {
                    int worldY = chunkGlobalY + y;
                    if (worldY < minDepth || worldY > maxDepth)
                    {
                        continue;
                    }

                    int worldX = chunkGlobalX + x;
                    if (!dataStore.IsInBounds(worldX, worldY))
                    {
                        continue;
                    }

                    if (dataStore.GetTileId(worldX, worldY, WorldTm.ForegroundTilemap) == stoneTileId)
                    {
                        localX = x;
                        localY = y;
                        return true;
                    }
                }
            }

            return false;
        }

        private void GrowOreClump(int startX, int startY, ushort oreTileId, int targetClumpSize, ushort stoneTileId, WorldDataStore dataStore, System.Random random)
        {
            if (!dataStore.IsInBounds(startX, startY))
            {
                return;
            }

            if (dataStore.GetTileId(startX, startY, WorldTm.ForegroundTilemap) != stoneTileId)
            {
                return;
            }

            dataStore.SetForegroundTileId(startX, startY, oreTileId);
            var placed = new HashSet<Vector2Int> { new Vector2Int(startX, startY) };
            var frontier = new List<Vector2Int>();
            var frontierSet = new HashSet<Vector2Int>();
            Vector2Int center = new Vector2Int(startX, startY);

            AddBlobFrontier(startX, startY, stoneTileId, dataStore, placed, frontier, frontierSet);
            float radius = Mathf.Max(2f, Mathf.Sqrt(targetClumpSize) * 1.4f);
            float radiusSq = radius * radius;

            while (placed.Count < targetClumpSize && frontier.Count > 0)
            {
                int index = SelectBlobFrontierIndex(frontier, center, radiusSq, random);
                Vector2Int candidate = frontier[index];
                frontierSet.Remove(candidate);
                frontier.RemoveAt(index);

                if (!dataStore.IsInBounds(candidate.x, candidate.y) || dataStore.GetTileId(candidate.x, candidate.y, WorldTm.ForegroundTilemap) != stoneTileId)
                {
                    continue;
                }

                dataStore.SetForegroundTileId(candidate.x, candidate.y, oreTileId);
                placed.Add(candidate);
                AddBlobFrontier(candidate.x, candidate.y, stoneTileId, dataStore, placed, frontier, frontierSet);
            }
        }

        private void AddBlobFrontier(int originX, int originY, ushort stoneTileId, WorldDataStore dataStore, HashSet<Vector2Int> placed, List<Vector2Int> frontier, HashSet<Vector2Int> frontierSet)
        {
            foreach (Vector2Int direction in AdjacentOffsets)
            {
                int neighborX = originX + direction.x;
                int neighborY = originY + direction.y;
                var neighborPos = new Vector2Int(neighborX, neighborY);

                if (placed.Contains(neighborPos) || frontierSet.Contains(neighborPos))
                {
                    continue;
                }

                if (!dataStore.IsInBounds(neighborX, neighborY))
                {
                    continue;
                }

                if (dataStore.GetTileId(neighborX, neighborY, WorldTm.ForegroundTilemap) != stoneTileId)
                {
                    continue;
                }

                frontier.Add(neighborPos);
                frontierSet.Add(neighborPos);
            }
        }

        private int SelectBlobFrontierIndex(List<Vector2Int> frontier, Vector2Int center, float radiusSq, System.Random random)
        {
            if (frontier.Count == 0)
            {
                return 0;
            }

            float totalWeight = 0f;
            var weights = new float[frontier.Count];

            for (int i = 0; i < frontier.Count; i++)
            {
                float distSq = (frontier[i] - center).sqrMagnitude;
                float distanceFactor = Mathf.Clamp01(distSq / radiusSq);
                float weight = Mathf.Lerp(2f, 0.5f, distanceFactor);
                weights[i] = weight;
                totalWeight += weight;
            }

            float choice = (float)random.NextDouble() * Mathf.Max(0.0001f, totalWeight);
            for (int i = 0; i < frontier.Count; i++)
            {
                if (choice <= weights[i])
                {
                    return i;
                }

                choice -= weights[i];
            }

            return frontier.Count - 1;
        }
    }
}
