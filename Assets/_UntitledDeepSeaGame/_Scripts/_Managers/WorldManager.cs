using System;
using System.Collections;
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
        public MultiTileManager MultiTileLifecycleManager { get; private set; }
        public bool IsWorldReady { get; private set; }
        public event Action OnWorldReady;

        [Header("Ocean Visuals")]
        [SerializeField] private OceanRenderer _oceanRenderer;
        [SerializeField] private OceanSurfaceRenderer _oceanSurfaceRenderer;
        [SerializeField] private ParallaxLayer _undergroundLayer;

        
        private void Awake()
        {
            Instance = this;
            
            WorldDataStore = WorldGenerator.GetComponent<WorldDataStore>();
            TileStreamingRenderer = WorldGenerator.GetComponent<WorldTileStreamingRenderer>();
            MultiTileLifecycleManager = GetComponent<MultiTileManager>();
            if (MultiTileLifecycleManager == null)
            {
                MultiTileLifecycleManager = gameObject.AddComponent<MultiTileManager>();
            }

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

            StartCoroutine(InitializeRuntimeWorldRoutine());
        }

        private IEnumerator InitializeRuntimeWorldRoutine()
        {
            WorldGenerationData generationData = WorldGenerator.GetComponent<WorldGenerationData>();

            IsWorldReady = false;
            WorldDataStore.Initialize(generationData.WorldWidth, generationData.WorldHeight, generationData.SeaLevelY);
            WorldGenerator.StartGeneration();

            while (!WorldGenerator.IsGenerationComplete)
            {
                yield return null;
            }

            TileStreamingRenderer.Initialize(WorldDataStore, WorldGenerator.ForegroundTilemap, WorldGenerator.BackgroundTilemap, WorldGenerator.AirTilemap, WorldGenerator.MultiTileRenderingTransform);
            MultiTileLifecycleManager?.Initialize(WorldDataStore);

            yield return new WaitUntil(() => Player.Instance != null);

            Vector3 spawnPosition = ResolveSpawnWorldPosition(WorldGenerator.SpawnTile);
            Player.Instance.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            Player.Instance.SpawnPoint = spawnPosition;
            Debug.Log($"Player spawned at {spawnPosition}");
            
            _oceanRenderer.Initialize(generationData);
            _oceanSurfaceRenderer.Initialize(generationData);
            _undergroundLayer.Initialize(generationData);
            
            StartCoroutine(InventoryManager.Instance.GiveStartingItems());

            IsWorldReady = true;
            OnWorldReady?.Invoke();
        }

        private Vector3 ResolveSpawnWorldPosition(Vector3Int spawnTile)
        {
            if (WorldGenerator.ForegroundTilemap != null)
            {
                Vector3 center = WorldGenerator.ForegroundTilemap.GetCellCenterWorld(spawnTile);
                return new Vector3(center.x, center.y, 0f);
            }

            return spawnTile;
        }

    }
}
