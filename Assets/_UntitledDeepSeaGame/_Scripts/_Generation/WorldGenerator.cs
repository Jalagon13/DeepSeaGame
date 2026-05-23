using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(WorldGenerationData))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Tilemap _forgroundTilemap;
    
        private WorldGenerationData _worldGenerationData;
        
        private void Awake() 
        {
            _worldGenerationData = GetComponent<WorldGenerationData>();
        }

        public void GenerateWorld()
        {
            Debug.Log($"Generating world data with seed {_worldGenerationData.Seed}");
            _worldGenerationData.ResetData();

            // Run generation steps
            foreach (Transform child in transform)
            {
                if(child.TryGetComponent(out GenerationStep step))
                {
                    step.Execute(_worldGenerationData);
                }
            }
            
            RenderWorld();
        }

        private void RenderWorld()
        {
            _forgroundTilemap.ClearAllTiles();

            int width = _worldGenerationData.TileData.GetLength(0);
            int height = _worldGenerationData.TileData.GetLength(1);
            Debug.Log($"Began rendering with width {width} and height {height}");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort tileId = _worldGenerationData.TileData[x, y];
                    TileSO tile = GameDataRegistry.Instance.GetTileSOFromTileId(tileId);
                    
                    _forgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
            
            Debug.Log($"Rendering world done");
        }
    }
}
