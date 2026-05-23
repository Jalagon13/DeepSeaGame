using System;
using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldManager : NetworkBehaviour
    {
        public static WorldManager Instance { get; private set; }

        [field: SerializeField] 
        public WorldGenerator WorldGenerator { get; private set; }

        public WorldDataStore WorldDataStore { get; private set; }
        public WorldTileStreamingRenderer TileStreamingRenderer { get; private set; }
        
        [SerializeField] 
        private Transform _spawnPoint;
        
        private void Awake()
        {
            Instance = this;
            
            WorldDataStore = WorldGenerator.GetComponent<WorldDataStore>();
            TileStreamingRenderer = WorldGenerator.GetComponent<WorldTileStreamingRenderer>();

            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnected;
            }
        }
        
        public override void OnDestroy() 
        {
            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.LocalClientId != clientId) return;

            InitializeRuntimeWorld();
            Player.Instance.transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
        }

        private void InitializeRuntimeWorld()
        {
            WorldGenerationData generationData = WorldGenerator.GetComponent<WorldGenerationData>();

            WorldDataStore.Initialize(generationData.WorldWidth, generationData.WorldHeight);
            WorldGenerator.GenerateWorldData();
            TileStreamingRenderer.Initialize(WorldDataStore, WorldGenerator.ForegroundTilemap);
        }
    }
}
