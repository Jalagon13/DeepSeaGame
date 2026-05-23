using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(WorldGenerationData), typeof(WorldDataStore), typeof(WorldTileStreamingRenderer))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private Tilemap _forgroundTilemap;

        public Tilemap ForegroundTilemap => _forgroundTilemap;

        private WorldGenerationData _worldGenerationData;
        private WorldDataStore _worldDataStore;

        private void Awake() 
        {
            _worldGenerationData = GetComponent<WorldGenerationData>();
            _worldDataStore = GetComponent<WorldDataStore>();
        }

        public void GenerateWorldData()
        {
            Debug.Log($"Generating world data with seed {_worldGenerationData.Seed}");

            foreach (Transform child in transform)
            {
                if(child.TryGetComponent(out GenerationStep step))
                {
                    step.Execute(_worldGenerationData, _worldDataStore);
                }
            }
        }
    }
}
