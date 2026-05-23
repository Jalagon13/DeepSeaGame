using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlaceSandGenStep : GenerationStep
    {
        [SerializeField] private TileSO _sandTileSO;
    
        public override void Execute(WorldGenerationData genData)
        {
            int width = genData.TileData.GetLength(0);
            int height = genData.TileData.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    genData.SetTileData(x, y, _sandTileSO);
                }
            }
            
            Debug.Log($"Place Sand Step done");
        }
    }
}
