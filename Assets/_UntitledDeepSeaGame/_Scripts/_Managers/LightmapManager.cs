using System;
using UnityEngine;
using UnityEngine.UI;

namespace UntitledDeepSeaGame
{
    public class LightmapManager : MonoBehaviour
    {
        public static LightmapManager Instance { get; private set; }
        
        [SerializeField] 
        private RawImage _lightmapOverlay;

        private RectInt _currentVisibleTileBounds;


        private void Awake()
        {
            Instance = this;
        }
        
        private void Start() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged += UpdateLightmap;
        }
        
        private void OnDestroy() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged -= UpdateLightmap;
        }

        private void UpdateLightmap(RectInt currentVisibleTileBounds)
        {
            UpdateOverlayRectTf(currentVisibleTileBounds);
            
            
        }

        private void UpdateOverlayRectTf(RectInt currentVisibleTileBounds)
        {
            // Convert tile positions to world space
            Vector2Int minWorldPos = currentVisibleTileBounds.min;
            Vector2Int maxWorldPos = currentVisibleTileBounds.max;

            // Calculate center and size in world space
            Vector2 centerWorldPos = (minWorldPos + maxWorldPos) / 2; // Center of the overlay
            Vector2 sizeWorld = new(maxWorldPos.x - minWorldPos.x, maxWorldPos.y - minWorldPos.y);

            // Update the RectTransform
            _lightmapOverlay.rectTransform.position = centerWorldPos; // Center position in world space
            _lightmapOverlay.rectTransform.sizeDelta = sizeWorld; // Set the scaled size in world units
            _lightmapOverlay.rectTransform.localScale = Vector3.one; // Keep scale uniform
        }



    }
}
