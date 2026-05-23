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
        
        [SerializeField] 
        private Transform _spawnPoint;
        
        private void Awake()
        {
            Instance = this;

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

            WorldGenerator.GenerateWorld();
            Player.Instance.transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
        }
    }
}
