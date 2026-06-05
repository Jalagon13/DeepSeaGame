using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlaceTitaniumOreStep : GenerationStep
    {
        [Header("Ore")]
        [SerializeField] private TileSO _titaniumOreTileSO;

        [Header("Clump Size")]
        [SerializeField] private int _minTilesPerClump = 6;
        [SerializeField] private int _maxTilesPerClump = 16;

        [Header("Clump Spacing")]
        [SerializeField] private int _minSpaceBetweenClumps = 12;
        [SerializeField] private int _maxSpaceBetweenClumps = 24;

        [Header("Shape Control")]
        [SerializeField, Range(0.1f, 5f)] private float _verticalScale = 1.3f;
        [SerializeField, Range(0f, 5f)] private float _randomnessStrength = 0.5f;

        public override WorldGenerationState State => WorldGenerationState.PlacingTitaniumOre;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            if (_titaniumOreTileSO == null)
            {
                Debug.LogError("Titanium Ore Tile SO is not assigned in PlaceTitaniumOreStep!");
                yield break;
            }

            int width = context.Config.WorldWidth;
            ushort titaniumTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_titaniumOreTileSO);

            // Start placing clumps.
            // We start scanning at a random offset based on clump spacing.
            int currentX = context.Random.Next(_minSpaceBetweenClumps, _maxSpaceBetweenClumps + 1);
            int prevMaxX = -1;

            while (currentX < width)
            {
                // Determine the size of this clump
                int tileCount = context.Random.Next(_minTilesPerClump, _maxTilesPerClump + 1);

                // Get the starting floor position (just above the sand floor)
                int surfaceHeight = context.SurfaceHeights[currentX];
                int yFloor = surfaceHeight + 1;

                int maxXPlaced = currentX;

                // Calculate the left-bound limit to enforce the spacing from the previous clump
                int minAllowedX = (prevMaxX == -1) ? 0 : prevMaxX + 1;

                // Place the clump at (currentX, yFloor)
                PlaceClump(context, currentX, yFloor, tileCount, titaniumTileId, minAllowedX, ref maxXPlaced);

                prevMaxX = maxXPlaced;

                // Determine next clump spacing
                int spacing = context.Random.Next(_minSpaceBetweenClumps, _maxSpaceBetweenClumps + 1);
                
                // Estimate next clump radius to offset center appropriately
                int nextTileCount = context.Random.Next(_minTilesPerClump, _maxTilesPerClump + 1);
                int estimatedRadius = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(nextTileCount / 3.14f)));

                // Advance currentX past the right edge of this clump plus spacing and next radius
                currentX = prevMaxX + spacing + estimatedRadius;

                // Yield to keep frame rate stable
                context.SetStepProgress((float)currentX / width);
                yield return null;
            }

            context.SetStepProgress(1f);
        }

        private void PlaceClump(WorldGenerationContext context, int xCenter, int yCenter, int tileCount, ushort titaniumTileId, int minAllowedX, ref int maxXPlaced)
        {
            HashSet<Vector2Int> clumpTiles = new HashSet<Vector2Int>();
            List<Vector2Int> candidates = new List<Vector2Int>();

            Vector2Int center = new Vector2Int(xCenter, yCenter);

            if (!context.DataStore.IsInBounds(center.x, center.y))
            {
                return;
            }

            // Add center
            clumpTiles.Add(center);
            context.DataStore.SetTileId(center.x, center.y, titaniumTileId);
            if (center.x > maxXPlaced)
            {
                maxXPlaced = center.x;
            }

            // Helper to check and add neighbor candidates
            void TryAddCandidate(Vector2Int pos)
            {
                if (!context.DataStore.IsInBounds(pos.x, pos.y)) return;
                // Clump stays on top of the sand (above the surface height)
                if (pos.y <= context.SurfaceHeights[pos.x]) return;
                
                // Enforce minimum horizontal distance/spacing from the previous clump
                if (pos.x < minAllowedX) return;

                if (clumpTiles.Contains(pos)) return;
                if (candidates.Contains(pos)) return;

                candidates.Add(pos);
            }

            // Add initial neighbors
            TryAddCandidate(new Vector2Int(center.x - 1, center.y));
            TryAddCandidate(new Vector2Int(center.x + 1, center.y));
            TryAddCandidate(new Vector2Int(center.x, center.y - 1));
            TryAddCandidate(new Vector2Int(center.x, center.y + 1));

            while (clumpTiles.Count < tileCount && candidates.Count > 0)
            {
                int bestIndex = -1;
                double bestScore = double.MaxValue;

                for (int i = 0; i < candidates.Count; i++)
                {
                    Vector2Int c = candidates[i];
                    double dx = c.x - center.x;
                    double dy = c.y - center.y;

                    // Apply vertical scale to control height-to-width ratio of the clump
                    double dist = Math.Sqrt(dx * dx + (dy * dy * _verticalScale * _verticalScale));

                    // Add randomness to make the shape natural and irregular
                    double randomOffset = context.Random.NextDouble() * _randomnessStrength;
                    double score = dist + randomOffset;

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }

                if (bestIndex != -1)
                {
                    Vector2Int chosen = candidates[bestIndex];
                    candidates.RemoveAt(bestIndex);

                    clumpTiles.Add(chosen);
                    context.DataStore.SetTileId(chosen.x, chosen.y, titaniumTileId);
                    if (chosen.x > maxXPlaced)
                    {
                        maxXPlaced = chosen.x;
                    }

                    // Add new neighbors
                    TryAddCandidate(new Vector2Int(chosen.x - 1, chosen.y));
                    TryAddCandidate(new Vector2Int(chosen.x + 1, chosen.y));
                    TryAddCandidate(new Vector2Int(chosen.x, chosen.y - 1));
                    TryAddCandidate(new Vector2Int(chosen.x, chosen.y + 1));
                }
            }
        }
    }
}
