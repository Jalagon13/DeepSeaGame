using UnityEngine;

namespace UntitledDeepSeaGame
{
    // WIP class but GE for now 
    public class Paralax : MonoBehaviour
    {
        [SerializeField, Tooltip("The speed at which the background moves horizontally relative to the camera")] 
        private float _paralaxEffect = 0.5f;

        [SerializeField, Tooltip("The speed at which the background moves vertically relative to the camera")] 
        private float _paralaxEffectY = 0.5f;
            
        private float _startPos, _length;
        private float _startYOffset;
        private float _startYPos;
        private float _spriteHeight;
        private float _cameraStartPosY;
        private bool _isInitializedY;
        private Transform _cameraTransform;
        private Camera _camera;
        
        private void Awake() 
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _length = spriteRenderer.bounds.size.x;
                _spriteHeight = spriteRenderer.bounds.size.y;
            }
        }
        
        private void Start() 
        {
            _startPos = transform.position.x;
            _startYPos = transform.position.y;
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _camera = Camera.main;
                _cameraStartPosY = _cameraTransform.position.y;
                _startYOffset = transform.position.y - _cameraTransform.position.y;
            }
        }
        
        private void FixedUpdate()
        {
            // Initialize Y tracking once the world is generated and player has snapped
            if (!_isInitializedY && WorldManager.Instance != null && WorldManager.Instance.IsWorldReady)
            {
                _cameraStartPosY = _cameraTransform.position.y;
                _startYPos = _cameraTransform.position.y;
                _isInitializedY = true;
            }

            // X Parallax (moves relative to camera X, with infinite repeating length)
            float distanceX = _cameraTransform.position.x * _paralaxEffect;
            float movementX = _cameraTransform.position.x * (1 - _paralaxEffect);
            
            // Y Parallax (moves relative to camera's displacement from start position, without repetition)
            float targetY = _cameraTransform.position.y + _startYOffset;
            if (_isInitializedY)
            {
                float cameraDeltaY = _cameraTransform.position.y - _cameraStartPosY;
                float distanceY = cameraDeltaY * _paralaxEffectY;
                targetY = _startYPos + distanceY;
            }

            // Clamp Y to prevent background from moving past its top/bottom edges relative to camera frustum
            float orthoSize = _camera.orthographicSize;
            if (_spriteHeight > 2f * orthoSize)
            {
                float minBgY = _cameraTransform.position.y + orthoSize - (_spriteHeight / 2f);
                float maxBgY = _cameraTransform.position.y - orthoSize + (_spriteHeight / 2f);
                targetY = Mathf.Clamp(targetY, minBgY, maxBgY);
            }
            else
            {
                // If the sprite is too small, lock to camera Y to prevent empty space
                targetY = _cameraTransform.position.y;
            }

            transform.position = new Vector3(_startPos + distanceX, targetY, transform.position.z);
            
            // If the background has reached the end of its length adjust its position for infinite scrolling
            if(movementX > _startPos + _length)
            {
                _startPos += _length;
            }
            else if(movementX < _startPos - _length)
            {
                _startPos -= _length;
            }
        }
    }
}
