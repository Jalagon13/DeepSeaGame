using FMODUnity;
using UnityEngine;

namespace DeepSeaGame
{
    public class HatchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private TileSO _swappedTile;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SpriteRenderer _sr;
        [SerializeField] private bool _isOpenHatch;
    
        public bool CanInteract => true;

        public void Interact()
        {
            Vector2Int anchor = Vector2Int.RoundToInt(transform.position);
            
            if (WorldManager.Instance == null || WorldManager.Instance.WorldDataStore == null)
            {
                return;
            }

            AudioManager.Instance.PlayOneShot(_isOpenHatch ? FMODEvents.Instance.HatchOpen : FMODEvents.Instance.HatchClosed, transform.position);

            if (WorldManager.Instance.WorldDataStore.ActiveMultiTiles.TryGetValue(anchor, out MultiTileData currentData))
            {
                if (_swappedTile != null)
                {
                    WorldManager.Instance.WorldDataStore.DestroyMultiTile(anchor.x, anchor.y);
                    WorldManager.Instance.WorldDataStore.SetMultiTile(anchor.x, anchor.y, _swappedTile, currentData.FlipX);
                }
            }
        }

        public void OnFlipX()
        {
            _sr.flipX = true;
            if(_isOpenHatch)
            {
                _collider.offset = new(0, _collider.offset.y);
            }
        }
    }
}
