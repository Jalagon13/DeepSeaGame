using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HatchInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract => true;

        public void Interact()
        {
            Vector2Int anchor = Vector2Int.RoundToInt(transform.position);
            
            if (WorldManager.Instance == null || WorldManager.Instance.WorldDataStore == null)
            {
                return;
            }

            Debug.Log($"Interacting with {gameObject.name}");

            var activeMultiTiles = WorldManager.Instance.WorldDataStore.ActiveMultiTileObjects;
            if (activeMultiTiles.TryGetValue(anchor, out TileSO currentTile))
            {
                if (currentTile.SwappedTile != null)
                {
                    WorldManager.Instance.WorldDataStore.DestroyMultiTile(anchor.x, anchor.y);
                    WorldManager.Instance.WorldDataStore.SetMultiTile(anchor.x, anchor.y, currentTile.SwappedTile);
                }
            }
        }
    }
}
