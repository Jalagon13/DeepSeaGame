using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class ResolveSpawnPointStep : GenerationStep
    {
        public override WorldGenerationState State => WorldGenerationState.FinalizingSpawn;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int centerX = context.Config.WorldWidth / 2;
            int maxOffset = context.Config.WorldWidth / 2;

            for (int offset = 0; offset <= maxOffset; offset++)
            {
                if (TryResolveColumn(context, centerX + offset, out Vector3Int spawnTile) ||
                    (offset > 0 && TryResolveColumn(context, centerX - offset, out spawnTile)))
                {
                    context.SpawnTile = spawnTile;
                    context.SetStepProgress(1f);
                    yield break;
                }

                if ((offset + 1) % context.Config.ColumnsPerFrame == 0)
                {
                    context.SetStepProgress((offset + 1f) / (maxOffset + 1));
                    yield return null;
                }
            }

            int fallbackSurface = context.SurfaceHeights[centerX];
            context.SpawnTile = new Vector3Int(centerX, fallbackSurface + 1, 0);
            context.SetStepProgress(1f);
        }

        private bool TryResolveColumn(WorldGenerationContext context, int x, out Vector3Int spawnTile)
        {
            spawnTile = default;

            if (x < 0 || x >= context.Config.WorldWidth)
            {
                return false;
            }

            int surfaceY = context.SurfaceHeights[x];
            int spawnY = surfaceY + 1;

            if (spawnY >= context.Config.WorldHeight)
            {
                return false;
            }

            if (context.DataStore.GetTileId(x, surfaceY) != context.SolidTileId)
            {
                return false;
            }

            if (context.DataStore.GetTileId(x, spawnY) != GameDataRegistry.INVALID_ID)
            {
                return false;
            }

            spawnTile = new Vector3Int(x, spawnY, 0);
            return true;
        }
    }
}
