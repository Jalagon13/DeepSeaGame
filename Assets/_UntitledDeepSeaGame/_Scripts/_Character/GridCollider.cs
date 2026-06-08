using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class GridCollider : MonoBehaviour
    {
        [SerializeField] 
        private BoxCollider2D _collider;
        
        private WorldDataStore _worldDataStore;
        
        private void Start() 
        {
            _worldDataStore = WorldManager.Instance.WorldDataStore;
        }
        
        private void FixedUpdate() 
        {
            
        }
    }
}