using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HatchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private TileSO _swappedTile;
    
        public bool CanInteract => true;

        public void Interact()
        {
            Vector2Int anchor = Vector2Int.RoundToInt(transform.position);
            
            if (WorldManager.Instance == null || WorldManager.Instance.WorldDataStore == null)
            {
                return;
            }

            Debug.Log($"Interacting with {gameObject.name}");

            if (WorldManager.Instance.WorldDataStore.ActiveMultiTileObjects.TryGetValue(anchor, out TileSO currentTile))
            {
                if (_swappedTile != null)
                {
                    WorldManager.Instance.WorldDataStore.DestroyMultiTile(anchor.x, anchor.y);
                    WorldManager.Instance.WorldDataStore.SetMultiTile(anchor.x, anchor.y, _swappedTile);
                }
            }
        }
    }
}
