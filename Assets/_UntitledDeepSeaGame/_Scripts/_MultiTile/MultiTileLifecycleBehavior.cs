using UnityEngine;

namespace UntitledDeepSeaGame
{
    public abstract class MultiTileLifecycleBehavior : ScriptableObject
    {
        public virtual void OnPlaced(MultiTileInstance instance, WorldDataStore dataStore) { }
        public virtual void OnRemoved(MultiTileInstance instance, WorldDataStore dataStore) { }
        public abstract void Update(MultiTileInstance instance, WorldDataStore dataStore, float deltaTime);
    }
}
