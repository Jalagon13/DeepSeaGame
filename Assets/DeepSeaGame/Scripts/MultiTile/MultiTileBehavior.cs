using UnityEngine;

namespace DeepSeaGame
{
    public abstract class MultiTileBehavior : ScriptableObject
    {
        public virtual void OnPlaced(MultiTileInstance instance, WorldDataStore dataStore) { }
        public virtual void OnRemoved(MultiTileInstance instance, WorldDataStore dataStore) { }
        public abstract void OnUpdate(MultiTileInstance instance, WorldDataStore dataStore, float deltaTime);
    }
}
