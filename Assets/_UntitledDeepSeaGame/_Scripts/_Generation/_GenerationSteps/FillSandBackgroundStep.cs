using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class FillSandBackgroundStep : GenerationStep
    {
        [Header("Fill Sand Background")]
        [SerializeField] private TileSO _sandWallTileSO;
        [SerializeField] private int _belowSurfaceOffset = 1;
        [SerializeField] private int _minWallPlacementYOffset = 12;
        [SerializeField] private int _maxWallPlacementYOffset = 17;
        
        [SerializeField, Range(0f, 1f)] 
        private float _approxUnderGroundStartPercent = 0.45f;

        public override WorldGenerationState State => WorldGenerationState.FillingTerrain;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int undergroundStartHeight = Mathf.RoundToInt(context.Config.WorldHeight * _approxUnderGroundStartPercent);
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            ushort sandWallTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_sandWallTileSO);

            for (int x = 0; x < width; x++)
            {
                int surfaceHeight = context.SurfaceHeights[x] - _belowSurfaceOffset;

                // Find the highest foreground sand tile that is flanked on both sides by sand.
                int flankedTopSandY = GetFlankedTopSandY(context, x, height);

                // Use the flanked top sand if found; otherwise fall back to the surface-based bound
                int upperBound = flankedTopSandY >= 0 ? flankedTopSandY - 1 : surfaceHeight;
                int minWallPlacementY = Mathf.Max(undergroundStartHeight, surfaceHeight - Random.Range(_minWallPlacementYOffset, _maxWallPlacementYOffset + 1));

                for (int y = 0; y < height; y++)
                {
                    if (y > undergroundStartHeight && y < upperBound)
                    {
                        context.DataStore.SetTileId(x, y, sandWallTileId, WorldTm.BackgroundTilemap);
                    }
                    else
                    {
                        context.DataStore.SetTileId(x, y, GameDataRegistry.INVALID_ID, WorldTm.BackgroundTilemap);
                    }
                    
                    if(y <= minWallPlacementY)
                    {
                        context.DataStore.SetTileId(x, y, sandWallTileId, WorldTm.BackgroundTilemap);
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
        
        private int GetFlankedTopSandY(WorldGenerationContext context, int x, int height)
        {
            ushort foregroundSolidId = context.SolidTileId;
            if (foregroundSolidId == GameDataRegistry.INVALID_ID) return -1;

            for (int y = height - 1; y >= 0; y--)
            {
                if (context.DataStore.GetTileId(x, y, WorldTm.ForegroundTilemap) != foregroundSolidId)
                {
                    continue;
                }

                // Must have neighbors on both sides and they must be the same solid tile
                int leftX = x - 1;
                int rightX = x + 1;
                if (leftX < 0 || rightX >= context.Config.WorldWidth) continue;

                if (context.DataStore.GetTileId(leftX, y, WorldTm.ForegroundTilemap) == foregroundSolidId && context.DataStore.GetTileId(rightX, y, WorldTm.ForegroundTilemap) == foregroundSolidId)
                {
                    return y;
                }
            }

            return -1;
        }
    }
}
