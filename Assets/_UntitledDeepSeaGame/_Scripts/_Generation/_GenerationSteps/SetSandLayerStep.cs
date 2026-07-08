using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class SetSandLayerStep : GenerationStep
    {
        [SerializeField] private TileSO _sandTileSO;
        [SerializeField] private int _depthOfSandLayer = 1;
        [SerializeField] private int _minYForSand = 250;

        public override WorldGenerationState State => WorldGenerationState.FillingTerrain;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            int width = context.Config.WorldWidth;
            int height = context.Config.WorldHeight;
            int seaLevelY = context.Config.SeaLevelY;
            int minY = Mathf.Clamp(_minYForSand, 0, height - 1);
            int sandDepth = Mathf.Max(1, _depthOfSandLayer);
            ushort sandTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_sandTileSO);

            for (int x = 0; x < width; x++)
            {
                for (int y = seaLevelY; y >= minY; y--)
                {
                    if (!context.DataStore.IsInBounds(x, y))
                    {
                        continue;
                    }

                    bool hasTile = context.DataStore.GetTileId(x, y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID;
                    bool hasTileAbove = y + 1 < height && context.DataStore.GetTileId(x, y + 1, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID;

                    if (!hasTile || hasTileAbove)
                    {
                        continue;
                    }

                    for (int depth = 0; depth < sandDepth; depth++)
                    {
                        int fillY = y - depth;
                        if (!context.DataStore.IsInBounds(x, fillY))
                        {
                            break;
                        }

                        context.DataStore.SetTileId(x, fillY, sandTileId, WorldTm.ForegroundTilemap);
                    }

                    y -= sandDepth - 1;
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
