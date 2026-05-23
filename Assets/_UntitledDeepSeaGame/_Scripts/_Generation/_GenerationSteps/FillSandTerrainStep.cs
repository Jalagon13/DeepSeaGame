using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class FillSandTerrainStep : GenerationStep
    {
        [Header("Terrain")]
        [SerializeField] private TileSO _sandTileSO;

        public override WorldGenerationState State => WorldGenerationState.FillingTerrain;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            ushort sandTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_sandTileSO);
            context.SolidTileId = sandTileId;

            for (int x = 0; x < width; x++)
            {
                int surfaceHeight = context.SurfaceHeights[x];
                for (int y = 0; y < height; y++)
                {
                    ushort tileId = y <= surfaceHeight ? sandTileId : GameDataRegistry.INVALID_ID;
                    context.DataStore.SetTileId(x, y, tileId);
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
