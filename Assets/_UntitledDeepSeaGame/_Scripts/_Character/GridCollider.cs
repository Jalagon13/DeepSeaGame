using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class GridCollider : MonoBehaviour
    {
        [SerializeField] 
        private BoxCollider2D _collider;
        
        [SerializeField]
        private float _skinWidth = 0.02f; // Tiny offset to prevent getting stuck in walls

        private WorldDataStore _worldDataStore;
        
        private void Start() 
        {
            _worldDataStore = WorldManager.Instance.WorldDataStore;
        }

        public Vector2 Move(Vector2 velocity, float deltaTime)
        {
            if (_worldDataStore == null) return velocity;

            Vector3 position = transform.position;

            // 1. Resolve X Axis
            float deltaX = velocity.x * deltaTime;
            if (Mathf.Abs(deltaX) > 0.0001f)
            {
                Bounds b = _collider.bounds;
                float direction = Mathf.Sign(deltaX);
                float xCheck = direction > 0 ? b.max.x + deltaX : b.min.x + deltaX;

                bool collision = false;
                // Check all tiles the side of the box overlaps
                for (int y = Mathf.FloorToInt(b.min.y + _skinWidth); y <= Mathf.FloorToInt(b.max.y - _skinWidth); y++)
                {
                    if (IsTileSolid(Mathf.FloorToInt(xCheck), y))
                    {
                        collision = true;
                        break;
                    }
                }

                if (collision)
                {
                    // Snap to the edge of the tile
                    position.x = direction > 0 ? Mathf.Floor(xCheck) - (_collider.size.x * 0.5f) - _collider.offset.x - _skinWidth 
                                               : Mathf.Ceil(xCheck) + (_collider.size.x * 0.5f) - _collider.offset.x + _skinWidth;
                    velocity.x = 0;
                }
                else position.x += deltaX;
            }

            transform.position = position; // Apply X before checking Y to allow sliding

            // 2. Resolve Y Axis
            float deltaY = velocity.y * deltaTime;
            if (Mathf.Abs(deltaY) > 0.0001f)
            {
                Bounds b = _collider.bounds;
                float direction = Mathf.Sign(deltaY);
                float yCheck = direction > 0 ? b.max.y + deltaY : b.min.y + deltaY;

                bool collision = false;
                for (int x = Mathf.FloorToInt(b.min.x + _skinWidth); x <= Mathf.FloorToInt(b.max.x - _skinWidth); x++)
                {
                    if (IsTileSolid(x, Mathf.FloorToInt(yCheck)))
                    {
                        collision = true;
                        break;
                    }
                }

                if (collision)
                {
                    position.y = direction > 0 ? Mathf.Floor(yCheck) - (_collider.size.y * 0.5f) - _collider.offset.y - _skinWidth 
                                               : Mathf.Ceil(yCheck) + (_collider.size.y * 0.5f) - _collider.offset.y + _skinWidth;
                    velocity.y = 0;
                }
                else position.y += deltaY;
            }

            transform.position = position;
            return velocity;
        }

        public bool IsGrounded()
        {
            if (_worldDataStore == null) return false;
            Bounds b = _collider.bounds;
            float yCheck = b.min.y - _skinWidth;
            
            for (int x = Mathf.FloorToInt(b.min.x + _skinWidth); x <= Mathf.FloorToInt(b.max.x - _skinWidth); x++)
            {
                if (IsTileSolid(x, Mathf.FloorToInt(yCheck))) return true;
            }
            return false;
        }

        private bool IsTileSolid(int x, int y)
        {
            if (!_worldDataStore.IsInBounds(x, y)) return true; // World bounds are solid
            return _worldDataStore.GetTileId(x, y, WorldTm.ForegroundTilemap) != GameDataRegistry.INVALID_ID;
        }
    }
}