using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class BiomeBackgroundManager : MonoBehaviour
    {
        [Header("Biome Layers Configuration")]
        [SerializeField]
        private List<BiomeBackgroundLayer> _layers = new List<BiomeBackgroundLayer>();

        [Header("Rendering Setup")]
        [SerializeField, Tooltip("Z-axis distance offset of background sprites relative to camera position")]
        private float _zOffset = 10f;

        private Transform _cameraTransform;
        private Camera _camera;

        private void Start()
        {
            InitializeLayers();
        }

        private void InitializeLayers()
        {
            foreach (var layer in _layers)
            {
                if (layer.backgroundSprite == null)
                {
                    Debug.LogWarning($"BiomeBackgroundManager: '{layer.layerName}' layer has no sprite assigned.");
                    continue;
                }

                // Create dynamic game object for this layer
                GameObject obj = new GameObject($"BiomeLayer_{layer.layerName}");
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.zero;

                // Setup SpriteRenderer
                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = layer.backgroundSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.tileMode = SpriteTileMode.Continuous;
                sr.sortingOrder = layer.sortingOrder;

                // Cache references
                layer.layerGameObject = obj;
                layer.spriteRenderer = sr;

                // Deactivate initially
                obj.SetActive(false);
            }
        }

        private void FixedUpdate()
        {
            // Cache camera references lazily if null
            if (_cameraTransform == null || _camera == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                    _camera = Camera.main;
                }
                else
                {
                    return;
                }
            }

            UpdateBackgroundLayers();
        }

        private void UpdateBackgroundLayers()
        {
            float cameraX = _cameraTransform.position.x;
            float cameraY = _cameraTransform.position.y;
            float orthoSize = _camera.orthographicSize;
            float cameraHeight = orthoSize * 2f;
            float cameraWidth = cameraHeight * _camera.aspect;

            foreach (var layer in _layers)
            {
                if (layer.layerGameObject == null || layer.spriteRenderer == null)
                {
                    continue;
                }

                // Check if camera frustum overlaps with layer vertical bounds
                bool isCameraInLayer = (cameraY + orthoSize >= layer.minY) && (cameraY - orthoSize <= layer.maxY);

                if (!isCameraInLayer)
                {
                    // Turn off layer when camera frustum is not overlapping it
                    if (layer.layerGameObject.activeSelf)
                    {
                        layer.layerGameObject.SetActive(false);
                    }
                    continue;
                }

                // Activate layer if currently disabled
                if (!layer.layerGameObject.activeSelf)
                {
                    layer.layerGameObject.SetActive(true);
                }

                // Keep fully opaque
                Color c = layer.spriteRenderer.color;
                layer.spriteRenderer.color = new Color(c.r, c.g, c.b, 1f);

                // Get sprite size
                float spriteWidth = layer.backgroundSprite.bounds.size.x;
                float spriteHeight = layer.backgroundSprite.bounds.size.y;

                if (spriteWidth <= 0f || spriteHeight <= 0f)
                {
                    continue;
                }

                // Tile size covers full camera viewport + extra padding to prevent seams.
                // Y padding uses a larger multiplier (6x = 3 tiles each side) because the background
                // snaps in spriteHeight-sized jumps vertically and needs enough buffer to never expose
                // the camera background colour during a snap frame.
                float targetWidth = cameraWidth + spriteWidth * 2f;
                float targetHeight = cameraHeight + spriteHeight * 6f;
                layer.spriteRenderer.size = new Vector2(targetWidth, targetHeight);

                // Calculate horizontal parallax with positive modulo (same as before)
                float distanceX = cameraX * layer.parallaxFactorX;
                float moduloOffsetX = GetPositiveModulo(distanceX, spriteWidth);

                // Calculate vertical world-space tile offset using the same modulo trick as X.
                // This snaps the background center to the nearest tile boundary in world space,
                // making it look like a static tiled world the player scrolls through
                // rather than a billboard following the camera.
                float moduloOffsetY = GetPositiveModulo(cameraY, spriteHeight);
                float snappedY = cameraY - moduloOffsetY;

                // Clamp snapped Y so the sprite edges respect the layer's minY and maxY world boundaries.
                float halfHeight = targetHeight / 2f;
                float minBgY = layer.minY + halfHeight;
                float maxBgY = layer.maxY - halfHeight;

                float targetY;
                if (minBgY > maxBgY)
                {
                    // Fallback: layer is shorter than the tiled sprite, center it in the layer
                    targetY = (layer.minY + layer.maxY) / 2f;
                }
                else
                {
                    targetY = Mathf.Clamp(snappedY, minBgY, maxBgY);
                }

                layer.layerGameObject.transform.position = new Vector3(
                    cameraX - moduloOffsetX, 
                    targetY, 
                    _cameraTransform.position.z + _zOffset
                );
            }
        }

        private float GetPositiveModulo(float value, float divisor)
        {
            float mod = value % divisor;
            return mod < 0f ? mod + divisor : mod;
        }
    }
}
