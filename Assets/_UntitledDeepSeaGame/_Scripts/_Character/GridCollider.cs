using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class GridCollider : MonoBehaviour
    {
        [SerializeField] 
        private BoxCollider2D _collider;

        [SerializeField, Tooltip("Optional: Use a specific collider for ground detection. If null, uses the main collider.")]
        private BoxCollider2D _feetCollider;
        
        [SerializeField]
        private float _skinWidth = 0.02f; // Tiny offset to prevent getting stuck in walls

        [SerializeField]
        private float _groundCheckDepth = 0.05f; // How far below the collider to look for ground

        private WorldDataStore _worldDataStore;
        
        private void Start() 
        {
            _worldDataStore = WorldManager.Instance.WorldDataStore;
        }

        public Vector2 Move(Vector2 velocity, float deltaTime)
        {
            if (_worldDataStore == null) return velocity;

            Vector2 currentPos = transform.position;
            
            // Calculate AABB manually to avoid stale 'collider.bounds' data after teleports
            Vector2 size = _collider.size;
            Vector2 offset = _collider.offset;
            Vector2 halfSize = size * 0.5f;

            // 1. Resolve X Axis
            float deltaX = velocity.x * deltaTime;
            if (Mathf.Abs(deltaX) > 0.0001f)
            {
                float direction = Mathf.Sign(deltaX);
                
                float xEdge = direction > 0 ? currentPos.x + offset.x + halfSize.x : currentPos.x + offset.x - halfSize.x;
                float xCheck = xEdge + deltaX;

                bool collision = false;
                
                int minGridY = Mathf.FloorToInt(currentPos.y + offset.y - halfSize.y + _skinWidth);
                int maxGridY = Mathf.FloorToInt(currentPos.y + offset.y + halfSize.y - _skinWidth);

                for (int y = minGridY; y <= maxGridY; y++)
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
                    currentPos.x = direction > 0 ? Mathf.Floor(xCheck) - halfSize.x - offset.x - _skinWidth 
                                               : Mathf.Ceil(xCheck) + halfSize.x - offset.x + _skinWidth;
                    velocity.x = 0;
                }
                else currentPos.x += deltaX;
            }

            // 2. Resolve Y Axis
            float deltaY = velocity.y * deltaTime;
            if (Mathf.Abs(deltaY) > 0.0001f)
            {
                float direction = Mathf.Sign(deltaY);
                
                float yEdge = direction > 0 ? currentPos.y + offset.y + halfSize.y : currentPos.y + offset.y - halfSize.y;
                float yCheck = yEdge + deltaY;

                bool collision = false;
                
                int minGridX = Mathf.FloorToInt(currentPos.x + offset.x - halfSize.x + _skinWidth);
                int maxGridX = Mathf.FloorToInt(currentPos.x + offset.x + halfSize.x - _skinWidth);

                for (int x = minGridX; x <= maxGridX; x++)
                {
                    if (IsTileSolid(x, Mathf.FloorToInt(yCheck)))
                    {
                        collision = true;
                        break;
                    }
                }

                if (collision)
                {
                    currentPos.y = direction > 0 ? Mathf.Floor(yCheck) - halfSize.y - offset.y - _skinWidth 
                                               : Mathf.Ceil(yCheck) + halfSize.y - offset.y + _skinWidth;
                    velocity.y = 0;
                }
                else currentPos.y += deltaY;
            }

            transform.position = new Vector3(currentPos.x, currentPos.y, 0f);
            return velocity;
        }

        public bool IsGrounded()
        {
            if (_worldDataStore == null) return false;

            // Use feet collider if available, otherwise fallback to main body collider
            BoxCollider2D target = (_feetCollider != null) ? _feetCollider : _collider;
            Vector2 pos = transform.position;
            Vector2 halfSize = target.size * 0.5f;

            // We check a tiny bit below the bottom edge of the chosen collider.
            float yCheck = pos.y + target.offset.y - halfSize.y - _groundCheckDepth;
            int gridY = Mathf.FloorToInt(yCheck);

            int minGridX = Mathf.FloorToInt(pos.x + target.offset.x - halfSize.x + _skinWidth);
            int maxGridX = Mathf.FloorToInt(pos.x + target.offset.x + halfSize.x - _skinWidth);

            for (int x = minGridX; x <= maxGridX; x++)
            {
                if (IsTileSolid(x, gridY)) return true;
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