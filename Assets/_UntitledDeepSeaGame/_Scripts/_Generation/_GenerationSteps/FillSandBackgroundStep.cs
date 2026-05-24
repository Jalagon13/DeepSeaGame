using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class FillSandBackgroundStep : GenerationStep
    {
        [Header("Terrain")]
        [SerializeField] private TileSO _sandWallTileSO;
        
        [SerializeField] private int _belowSurfaceOffset = 3;

        public override WorldGenerationState State => WorldGenerationState.FillingTerrain;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            ushort sandWallTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_sandWallTileSO);

            for (int x = 0; x < width; x++)
            {
                int surfaceHeight = context.SurfaceHeights[x] - _belowSurfaceOffset;
                for (int y = 0; y < height; y++)
                {
                    ushort tileId = y <= surfaceHeight ? sandWallTileId : GameDataRegistry.INVALID_ID;
                    context.DataStore.SetTileId(x, y, tileId, WorldTm.BackgroundTilemap);
                }

                if ((x + 1) % context.Config.ColumnsPerFrame == 0)
                {
                    context.SetStepProgress((x + 1f) / width);
                    yield return null;
                }
            }

            context.SetStepProgress(1f);
        }
    }
}
