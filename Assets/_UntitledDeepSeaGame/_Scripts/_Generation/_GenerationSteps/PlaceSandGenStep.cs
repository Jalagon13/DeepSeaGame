using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlaceSandGenStep : GenerationStep
    {
        [SerializeField] private TileSO _sandTileSO;
    
        public override void Execute(WorldGenerationData genData, WorldDataStore worldDataStore)
        {
            int width = worldDataStore.Width;
            int height = worldDataStore.Height;
            ushort sandTileId = GameDataRegistry.Instance.GetTileIdFromTileSO(_sandTileSO);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    worldDataStore.SetTileId(x, y, sandTileId);
                }
            }
            
            Debug.Log($"Place Sand Step done");
        }
    }
}
