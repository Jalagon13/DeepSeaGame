using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class UndergroundBackground : MonoBehaviour
    {
        [SerializeField] private int _layerMaxY;
        [SerializeField] private int _layerMinY;

        [SerializeField, Range(0f, 1f), Tooltip("Horizontal parallax scrolling speed factor (0 = moves fully with camera, 1 = locked to world)")]
        private float _parallaxFactorX = 0.5f;

        private SpriteRenderer _backgroundSr;
        private Vector2 _baseSize;
        
        private void Awake() 
        {
            _backgroundSr = GetComponent<SpriteRenderer>();
            
            // Debug.Log($"Size: {_backgroundSr.size}");
            _baseSize = new Vector2(_backgroundSr.size.x, _backgroundSr.size.y);
        }
        
        private void Start() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged += HandleVisibleTileBoundsChanged;
        }
        
        private void OnDestroy() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged -= HandleVisibleTileBoundsChanged;
        }

        private void HandleVisibleTileBoundsChanged(RectInt bounds)
        {
            // Check if camera frustum overlaps with layer vertical bounds.
            // Adding a buffer ensures the background is ready before the camera arrives.
            float buffer = _baseSize.y > 0 ? _baseSize.y : 10f;
            bool isCameraInLayer = (bounds.yMax >= _layerMinY - buffer) && (bounds.yMin <= _layerMaxY + buffer);
            
            if (!isCameraInLayer)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }

            if (Player.Instance == null || _baseSize.x <= 0f || _baseSize.y <= 0f)
                return;

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            // Calculate a size that is a multiple of _baseSize and large enough to cover the camera frustum
            // Adding a buffer of 2 base units ensures no gaps appear when the position snaps to the grid
            int multiplierX = Mathf.CeilToInt(bounds.width / _baseSize.x) + 2;
            int multiplierY = Mathf.CeilToInt(bounds.height / _baseSize.y) + 2;

            _backgroundSr.size = new Vector2(multiplierX * _baseSize.x, multiplierY * _baseSize.y);
        }

        private void LateUpdate()
        {
            if (!gameObject.activeSelf || Player.Instance == null || _baseSize.x <= 0f || _baseSize.y <= 0f)
                return;

            Vector3 playerPos = Player.Instance.transform.position;

            // Horizontal parallax: Keep the quad centered on player, but shift it by the parallax offset (modulo baseSize)
            // This creates a smooth sliding effect while ensuring the quad stays under the camera.
            float horizontalOffset = Mathf.Repeat(playerPos.x * _parallaxFactorX, _baseSize.x);
            float targetX = playerPos.x - horizontalOffset;

            // Vertical: Snapped to multiples of _baseSize.y and clamped to biome bounds (with coverage bleed)
            float snappedY = Mathf.Round(playerPos.y / _baseSize.y) * _baseSize.y;
            float halfHeight = _backgroundSr.size.y / 2f;

            float minSnappedY = Mathf.Floor((_layerMinY + halfHeight) / _baseSize.y) * _baseSize.y;
            float maxSnappedY = Mathf.Ceil((_layerMaxY - halfHeight) / _baseSize.y) * _baseSize.y;

            float finalY = (minSnappedY > maxSnappedY) 
                ? Mathf.Round(((_layerMinY + _layerMaxY) / 2f) / _baseSize.y) * _baseSize.y 
                : Mathf.Clamp(snappedY, minSnappedY, maxSnappedY);

            _backgroundSr.transform.position = new Vector3(targetX, finalY, _backgroundSr.transform.position.z);
        }
    }
}
