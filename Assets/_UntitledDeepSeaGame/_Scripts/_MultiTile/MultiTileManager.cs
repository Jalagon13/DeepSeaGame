using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class MultiTileManager : MonoBehaviour
    {
        private WorldDataStore _worldDataStore;
        private readonly Dictionary<Vector2Int, MultiTileInstance> _activeInstances = new();
        private bool _isInitialized;

        public void Initialize(WorldDataStore worldDataStore)
        {
            if (worldDataStore == null)
            {
                Debug.LogWarning("MultiTileLifecycleManager.Initialize called with null WorldDataStore.");
                return;
            }

            if (_isInitialized)
            {
                _worldDataStore.MultiTileChanged -= HandleMultiTileChanged;
                _activeInstances.Clear();
            }

            _worldDataStore = worldDataStore;
            _worldDataStore.MultiTileChanged += HandleMultiTileChanged;
            _isInitialized = true;

            foreach (var kvp in _worldDataStore.ActiveMultiTileObjects)
            {
                CreateInstance(kvp.Key, kvp.Value);
            }
        }

        private void OnDestroy()
        {
            if (_worldDataStore != null)
            {
                _worldDataStore.MultiTileChanged -= HandleMultiTileChanged;
            }
        }

        private void Update()
        {
            if (!_isInitialized || _worldDataStore == null || WorldManager.Instance == null || !WorldManager.Instance.IsWorldReady)
            {
                return;
            }

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            foreach (var instance in _activeInstances.Values)
            {
                instance.Behavior?.Update(instance, _worldDataStore, deltaTime);
            }
        }

        private void HandleMultiTileChanged(Vector2Int anchor, TileSO multiTile, bool isPlacingMultiTile)
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
            if (!_activeInstances.ContainsKey(anchor))
            {
                MultiTileBehavior behavior = tileSO?.MultiTileLifecycleBehavior;
                var instance = new MultiTileInstance(anchor, tileSO, behavior);
                _activeInstances.Add(anchor, instance);
                behavior?.OnPlaced(instance, _worldDataStore);
            }
        }

        private void RemoveInstance(Vector2Int anchor)
        {
            if (_activeInstances.TryGetValue(anchor, out MultiTileInstance instance))
            {
                instance.Behavior?.OnRemoved(instance, _worldDataStore);
                _activeInstances.Remove(anchor);
            }
        }
    }
}
