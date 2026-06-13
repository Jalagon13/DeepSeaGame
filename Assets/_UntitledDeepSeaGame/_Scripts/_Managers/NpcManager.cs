using System.Collections.Generic;
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

        [SerializeField, Tooltip("How many NPCs spawn per minute in this biome"), Range(0f, 60f)] 
        private float _spawnsPerMinute;

        [Header("Test Npc stuff")]
        [SerializeField] 
        private CharacterSO _testNpc;

        [Header("Spawning Range (in Tiles)")]
        [SerializeField, Tooltip("Inner rectangle bounds where mobs CANNOT spawn (No-Spawn Zone).")]
        private Vector2Int _innerNoSpawnDimensions = new Vector2Int(124, 70);

        [SerializeField, Tooltip("Outer rectangle bounds within which mobs CAN spawn.")]
        private Vector2Int _outerSpawnDimensions = new Vector2Int(168, 94);

        [Header("Global Spawning Caps")]
        [SerializeField, Tooltip("Maximum number of NPCs that can exist in the world at once.")]
        private int _globalMaxNpcCap = 200;

        private readonly float _tickTime = 1f / 60f; // 60 ticks per second
        private readonly int _maxSpawnAttempts = 50;

        private readonly Dictionary<ulong, PlayerSpawnData> _playerSpawnData = new();
        private class PlayerSpawnData
        {
            public float CurrentCapacity;
            public int MaxNpcSlotAmount;
            public readonly List<ServerCharacter> SpawnedNpcs = new();
        }


        private void Awake()
        {
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
                NetworkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

                // Initialize spawn data for already connected players (e.g. host)
                foreach (var client in NetworkManager.ConnectedClientsList)
                {
                    if (!_playerSpawnData.ContainsKey(client.ClientId))
                    {
                        _playerSpawnData[client.ClientId] = new PlayerSpawnData
                        {
                            MaxNpcSlotAmount = _maxNpcSlotAmount
                        };
                    }
                }

                InvokeRepeating(nameof(TryToSpawnNpc), _startSpawnDelay, _tickTime);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
                NetworkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void NetworkManager_OnClientConnectedCallback(ulong clientId)
        {
            if (!IsServer) return;

            if (!_playerSpawnData.ContainsKey(clientId))
            {
                _playerSpawnData[clientId] = new PlayerSpawnData
                {
                    MaxNpcSlotAmount = _maxNpcSlotAmount
                };
            }
        }

        private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
        {
            if (!IsServer) return;
            _playerSpawnData.Remove(clientId);
        }

        // I think this is totally local to the server right now not for other clients becareful
        private int GetGlobalActiveNpcCount()
        {
            int count = 0;
            foreach (var kvp in _playerSpawnData)
            {
                count += kvp.Value.SpawnedNpcs.Count;
            }
            return count;
        }

        private void RecalculatePlayerCapacity(PlayerSpawnData spawnData)
        {
            float cap = 0f;
            for (int i = spawnData.SpawnedNpcs.Count - 1; i >= 0; i--)
            {
                ServerCharacter npc = spawnData.SpawnedNpcs[i];
                if (npc == null || npc.LifeState == LifeState.Dead)
                {
                    spawnData.SpawnedNpcs.RemoveAt(i);
                    continue;
                }
                cap += npc.CharacterData.SlotAmount;
            }
            spawnData.CurrentCapacity = cap;
        }

        public void TryToSpawnNpc()
        {
            if (!IsServer || !_enableSpawning || !WorldManager.Instance.IsWorldReady) return;

            // Check global NPC cap
            if (GetGlobalActiveNpcCount() >= _globalMaxNpcCap) return;

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;

                ulong playerId = client.ClientId;
                Transform playerTransform = client.PlayerObject.transform;

                if (!_playerSpawnData.TryGetValue(playerId, out PlayerSpawnData spawnData))
                {
                    spawnData = new PlayerSpawnData { MaxNpcSlotAmount = _maxNpcSlotAmount };
                    _playerSpawnData[playerId] = spawnData;
                }

                // Clean up and recalculate this player's active NPC capacity
                RecalculatePlayerCapacity(spawnData);

                if (spawnData.CurrentCapacity >= spawnData.MaxNpcSlotAmount) continue;

                // Calculate spawn probability per tick (Terraria-style)
                float spawnModifier = GetSpawnModifier(spawnData.CurrentCapacity, spawnData.MaxNpcSlotAmount);
                float spawnsPerMinute = _spawnsPerMinute;

                // Convert spawns per minute to probability per tick
                float spawnProbability = (spawnsPerMinute / 3600f) * spawnModifier;

                // Roll for spawn attempt
                if (Random.value < spawnProbability)
                {
                    // Try to find a valid spawn spot (Terraria-style: limited attempts per tick)
                    for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
                    {
                        Vector2 potentialSpawnPoint = GetRandomTileInSpawnArea(playerTransform.position);

                        if (SpawnSpotIsValid(potentialSpawnPoint))
                        {
                            float remainingNpcSlotSpace = spawnData.MaxNpcSlotAmount - spawnData.CurrentCapacity;
                            CharacterSO npcToSpawn = GetNpcToSpawn();

                            if (npcToSpawn.SlotAmount <= remainingNpcSlotSpace)
                            {
                                SpawnNpcOnServer(potentialSpawnPoint, npcToSpawn, playerId);
                                break; // Successfully spawned, exit attempts loop
                            }
                        }
                    }
                }
            }
        }

        private void SpawnNpcOnServer(Vector2 position, CharacterSO npcToSpawn, ulong playerId)
        {
            if (!IsServer) return;

            var spawnPosition = new Vector2(Mathf.FloorToInt(position.x) + 0.5f, Mathf.FloorToInt(position.y) + 0.5f);
            GameObject npcPrefab = Instantiate(npcToSpawn.NpcPrefab.gameObject, spawnPosition, Quaternion.identity);

            NetworkObject npcPrefabNetworkObject = npcPrefab.GetComponent<NetworkObject>();
            npcPrefabNetworkObject.SpawnWithObservers = false;
            npcPrefabNetworkObject.Spawn();

            if (npcPrefab.TryGetComponent<ServerCharacter>(out var serverCharacter))
            {
                if (_playerSpawnData.TryGetValue(playerId, out PlayerSpawnData spawnData))
                {
                    spawnData.SpawnedNpcs.Add(serverCharacter);
                    RecalculatePlayerCapacity(spawnData);
                }
            }

            Debug.Log($"SpawnNpcOnServer: {npcPrefab.name} at {position} for player {playerId}");
        }

        private CharacterSO GetNpcToSpawn()
        {
            return _testNpc;
        }

        private bool SpawnSpotIsValid(Vector2 potentialSpawnPoint)
        {
            // Check if the tile is within the camera's visible screen bounds to prevent pop-in
            if (PlayerCamera.Instance != null)
            {
                Vector2Int tileCoords = new Vector2Int(Mathf.FloorToInt(potentialSpawnPoint.x), Mathf.FloorToInt(potentialSpawnPoint.y));

                if (PlayerCamera.Instance.CurrentVisibleTileBounds.Contains(tileCoords))
                {
                    return false; // Point is visible on screen!
                }
            }

            return true;
        }

        private Vector2 GetRandomTileInSpawnArea(Vector2 playerPos)
        {
            float halfInnerX = _innerNoSpawnDimensions.x / 2f;
            float halfInnerY = _innerNoSpawnDimensions.y / 2f;
            float halfOuterX = _outerSpawnDimensions.x / 2f;
            float halfOuterY = _outerSpawnDimensions.y / 2f;

            // Attempt to find a point in the donut area
            for (int i = 0; i < 10; i++)
            {
                float rx = Random.Range(-halfOuterX, halfOuterX);
                float ry = Random.Range(-halfOuterY, halfOuterY);

                // If it falls inside the inner no-spawn rectangle, re-roll
                if (Mathf.Abs(rx) < halfInnerX && Mathf.Abs(ry) < halfInnerY)
                {
                    continue;
                }

                return new Vector2(playerPos.x + rx, playerPos.y + ry);
            }

            return default;
        }

        private float GetSpawnModifier(float currentCapacity, int maxCapacity)
        {
            if (maxCapacity <= 0) return 0.1f;
            float activeRatio = currentCapacity / maxCapacity;

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
