using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class NpcManager : NetworkBehaviour
    {
        public static NpcManager Instance { get; private set; }

        [SerializeField]
        private bool _enableSpawning = true;

        [SerializeField]
        private float _startSpawnDelay;
        
        [SerializeField] 
        private int _maxNpcSlotAmount = 6;

        [field: SerializeField, Tooltip("How many NPCs spawn per minute in this biome"), Range(0f, 60f)]
        public float SpawnsPerMinute { get; private set; }
        
        [SerializeField, Tooltip("How many NPCs spawn per minute in this biome"), Range(0f, 60f)] 
        private float _spawnsPerMinute;

        private readonly float _tickTime = 1f / 60f; // 60 ticks per second
        private readonly int _maxSpawnAttempts = 50;
        private Transform _localPlayerTransform;
        private float _currentNpcCapacity = 0;

        private void Awake()
        {
            Instance = this;

            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            }
        }

        public override void OnDestroy()
        {
            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
            }

            base.OnDestroy();
        }

        private void NetworkManager_OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.LocalClientId != clientId) return;

            _localPlayerTransform = NetworkManager.ConnectedClients[clientId].PlayerObject.transform;

            InvokeRepeating(nameof(TryToSpawnNpc), _startSpawnDelay, _tickTime);

        }

        public void TryToSpawnNpc()
        {
            if (!_enableSpawning || _localPlayerTransform == null || !WorldManager.Instance.IsWorldReady) return;

            // Check if we're at max capacity
            if (_currentNpcCapacity >= _maxNpcSlotAmount) return;

            // Calculate spawn probability per tick (Terraria-style)
            float spawnModifier = GetSpawnModifier();
            float spawnsPerMinute = _spawnsPerMinute;

            // Convert spawns per minute to probability per tick
            // If we want X spawns per minute and we tick 60 times per second (3600 times per minute)
            // Then probability per tick = X / 3600 * modifier
            float spawnProbability = (spawnsPerMinute / 3600f) * spawnModifier;

            // Roll for spawn attempt
            if (Random.value < spawnProbability)
            {
                // Try to find a valid spawn spot (Terraria-style: limited attempts per tick)
                for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
                {
                    Vector2 potentialSpawnPoint = GetRandomTileInSpawnArea();

                    if (SpawnSpotIsValid(potentialSpawnPoint))
                    {
                        float remainingNpcSlotSpace = _maxNpcSlotAmount - _currentNpcCapacity;
                        
                        SpawnNpc(potentialSpawnPoint);
                    }
                }
            }
        }

        public void SpawnNpc(Vector2 spawnPosition)
        {
           
        }

        private bool SpawnSpotIsValid(Vector2 potentialSpawnPoint)
        {
            return true;
        }

        private Vector2 GetRandomTileInSpawnArea()
        {
            return default;
        }

        private float GetSpawnModifier()
        {
            float activeRatio = _currentNpcCapacity / _maxNpcSlotAmount;

            // Terraria-style: More mobs = lower spawn rate, fewer mobs = higher spawn rate
            if (activeRatio < 0.2f)
            {
                return 1.5f; // 50% faster when area is mostly empty
            }
            else if (activeRatio < 0.4f)
            {
                return 1.3f; // 30% faster when area is 20-40% full
            }
            else if (activeRatio < 0.6f)
            {
                return 1.1f; // 10% faster when area is 40-60% full
            }
            else if (activeRatio < 0.8f)
            {
                return 0.9f; // 10% slower when area is 60-80% full
            }
            else if (activeRatio < 0.95f)
            {
                return 0.5f; // 50% slower when area is 80-95% full
            }

            return 0.1f; // 90% slower when area is nearly full
        }
    }
}
