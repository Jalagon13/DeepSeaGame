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
        public bool IsWorldReady { get; private set; }

        [Header("Ocean Visuals")]
        [SerializeField] private bool _createDefaultOceanRenderers = true;
        [SerializeField] private OceanRenderer _oceanRenderer;
        [SerializeField] private OceanSurfaceRenderer _oceanSurfaceRenderer;

        public event Action OnWorldReady;
        
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

            StartCoroutine(InitializeRuntimeWorldRoutine());
        }

        private IEnumerator InitializeRuntimeWorldRoutine()
        {
            WorldGenerationData generationData = WorldGenerator.GetComponent<WorldGenerationData>();

            IsWorldReady = false;
            WorldDataStore.Initialize(generationData.WorldWidth, generationData.WorldHeight, generationData.SeaLevelY);
            EnsureOceanRenderers(generationData);
            WorldGenerator.StartGeneration();

            while (!WorldGenerator.IsGenerationComplete)
            {
                yield return null;
            }

            TileStreamingRenderer.Initialize(WorldDataStore, WorldGenerator.ForegroundTilemap, WorldGenerator.BackgroundTilemap, WorldGenerator.AirTilemap, WorldGenerator.MultiTileRenderingTransform);

            yield return new WaitUntil(() => Player.Instance != null);

            Vector3 spawnPosition = ResolveSpawnWorldPosition(WorldGenerator.SpawnTile);
            Player.Instance.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            Player.Instance.SpawnPoint = spawnPosition;
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

        private void EnsureOceanRenderers(WorldGenerationData generationData)
        {
            if (!_createDefaultOceanRenderers)
            {
                _oceanRenderer?.Initialize(generationData);
                _oceanSurfaceRenderer?.Initialize(generationData);
                return;
            }

            if (_oceanRenderer == null)
            {
                _oceanRenderer = FindAnyObjectByType<OceanRenderer>();
            }

            if (_oceanRenderer == null)
            {
                GameObject oceanGo = new("OceanRenderer", typeof(SpriteRenderer), typeof(OceanRenderer));
                oceanGo.transform.SetParent(WorldGenerator.transform, false);
                _oceanRenderer = oceanGo.GetComponent<OceanRenderer>();
            }

            if (_oceanSurfaceRenderer == null)
            {
                _oceanSurfaceRenderer = FindAnyObjectByType<OceanSurfaceRenderer>();
            }

            if (_oceanSurfaceRenderer == null)
            {
                GameObject surfaceGo = new("OceanSurfaceRenderer", typeof(SpriteRenderer), typeof(OceanSurfaceRenderer));
                surfaceGo.transform.SetParent(WorldGenerator.transform, false);
                _oceanSurfaceRenderer = surfaceGo.GetComponent<OceanSurfaceRenderer>();
            }

            _oceanRenderer.Initialize(generationData);
            _oceanSurfaceRenderer.Initialize(generationData);
        }
    }
}
