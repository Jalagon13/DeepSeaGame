using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace DeepSeaGame
{
    public class MultiTileManager : MonoBehaviour
    {
        public static MultiTileManager Instance { get; private set; }
    
        private readonly Dictionary<Vector2Int, MultiTileInstance> _activeInstances = new();

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start() 
        {
            WorldManager.Instance.WorldDataStore.MultiTileChanged += HandleMultiTileChanged;
        }

        private void OnDestroy()
        {
            WorldManager.Instance.WorldDataStore.MultiTileChanged -= HandleMultiTileChanged;
        }

        private void Update()
        {
            if (WorldManager.Instance == null || !WorldManager.Instance.IsWorldReady || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            foreach (MultiTileInstance instance in _activeInstances.Values)
            {
                instance.Behavior.Update(instance, WorldManager.Instance.WorldDataStore, Time.deltaTime);
            }
        }

        private void HandleMultiTileChanged(Vector2Int anchor, TileSO multiTile, bool isPlacingMultiTile, bool flipX)
        {
            if (isPlacingMultiTile)
            {
                CreateInstance(anchor, multiTile);
            }
            else
            {
                RemoveInstance(anchor);
            }
        }

        private void CreateInstance(Vector2Int anchor, TileSO tileSO)
        {
            if (!_activeInstances.ContainsKey(anchor) && tileSO != null && tileSO.IsMultiTile && tileSO.Behavior != null)
            {
                MultiTileBehavior behavior = tileSO.Behavior;
                MultiTileInstance instance = new(anchor, tileSO, behavior);
                _activeInstances.Add(anchor, instance);
                behavior.OnPlaced(instance, WorldManager.Instance.WorldDataStore);
            }
        }

        private void RemoveInstance(Vector2Int anchor)
        {
            if (_activeInstances.TryGetValue(anchor, out MultiTileInstance instance))
            {
                instance.Behavior.OnRemoved(instance, WorldManager.Instance.WorldDataStore);
                _activeInstances.Remove(anchor);
            }
        }
    }
}
