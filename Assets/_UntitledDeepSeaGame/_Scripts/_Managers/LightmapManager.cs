using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UntitledDeepSeaGame
{
    /// <summary>
    /// Manages a CPU-based 2D lightmap using a Breadth-First Search (BFS) flood fill algorithm.
    /// Propagates light based on tile solid/ambient attenuation and outputs a grayscale texture overlay.
    /// </summary>
    public class LightmapManager : MonoBehaviour
    {
        public static LightmapManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] 
        private RawImage _lightmapOverlay;

        [Header("Light Attenuation Settings")]
        [Tooltip("How what value the brightest tile is")]
        [SerializeField]
        private float _fullBrightnessInterpretation = 15f;

        [Tooltip("How much light dims when propagating into a solid foreground tile.")]
        [SerializeField] 
        private float _solidForegroundAttenuation = 1.0f;

        [Tooltip("How much light dims when propagating into a background-only tile.")]
        [SerializeField] 
        private float _backgroundOnlyAttenuation = 0.5f;

        [Tooltip("How much light dims when propagating into empty air (no foreground, no background).")]
        [SerializeField] 
        private float _airOnlyAttenuation = 0.5f;

        [Header("Texture Settings")]
        [Tooltip("The filter mode for the lightmap overlay texture (Point for pixelated tiles, Bilinear for smooth).")]
        [SerializeField] 
        private FilterMode _lightmapFilterMode = FilterMode.Point;

        [Header("Blending Properties")]
        [Tooltip("Enable multiply blending to darken the scene. If disabled, renders the raw grayscale texture.")]
        [SerializeField] 
        private bool _enableMultiplyBlending = true;

        [Tooltip("Optional custom material using the Multiply shader. If left empty, one will be created dynamically at runtime.")]
        [SerializeField] 
        private Material _multiplyMaterial;

        [Header("Padding Settings")]
        [Tooltip("Extra padding (in tiles) around the camera frustum for light calculations. Prevents lighting pop-in on screen edges.")]
        [SerializeField] 
        private int _extraLightmapPadding = 8;

        // Cached runtime variables to completely eliminate GC garbage collection overhead
        private WorldDataStore _worldDataStore;
        private float[,] _lightGrid;
        private int _gridWidth;
        private int _gridHeight;
        
        private Texture2D _lightmapTexture;
        private Color32[] _colorBuffer;
        private readonly Queue<Vector2Int> _bfsQueue = new();

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start() 
        {
            // Subscribe to the camera visible tile bounds change event
            PlayerCamera.OnVisibleTileBoundsChanged += UpdateLightmap;
        }
        
        private void OnDestroy() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged -= UpdateLightmap;
            
            // Clean up resources to prevent memory leaks
            if (_lightmapTexture != null)
            {
                Destroy(_lightmapTexture);
            }
        }

        /// <summary>
        /// Main lightmap recalculation logic called whenever the camera viewport streams new tiles.
        /// </summary>
        /// <param name="currentVisibleTileBounds">The RectInt defining the current camera frustum in tile coords.</param>
        private void UpdateLightmap(RectInt currentVisibleTileBounds)
        {
            // Verify world references and readiness
            if (WorldManager.Instance == null || !WorldManager.Instance.IsWorldReady)
            {
                return;
            }

            if (_worldDataStore == null)
            {
                _worldDataStore = WorldManager.Instance.WorldDataStore;
                if (_worldDataStore == null) return;
            }

            // Inflate bounds by the padding in all four directions to prevent pop-in on screen edges
            int minX = currentVisibleTileBounds.xMin - _extraLightmapPadding;
            int minY = currentVisibleTileBounds.yMin - _extraLightmapPadding;
            int maxX = currentVisibleTileBounds.xMax + _extraLightmapPadding;
            int maxY = currentVisibleTileBounds.yMax + _extraLightmapPadding;
            RectInt inflatedBounds = new(minX, minY, maxX - minX, maxY - minY);

            int width = inflatedBounds.width;
            int height = inflatedBounds.height;

            if (width <= 0 || height <= 0) return;

            // Step 1: Manage buffer sizes and allocate/resize only when dimensions change (eliminates GC spikes)
            if (_lightGrid == null || _gridWidth != width || _gridHeight != height)
            {
                _lightGrid = new float[width, height];
                _gridWidth = width;
                _gridHeight = height;

                if (_lightmapTexture != null)
                {
                    Destroy(_lightmapTexture);
                }

                // Initialize a standard 2D grayscale texture with bilinear/point filtering and clamped borders
                _lightmapTexture = new Texture2D(width, height, TextureFormat.RGB24, false)
                {
                    filterMode = _lightmapFilterMode,
                    wrapMode = TextureWrapMode.Clamp
                };

                _colorBuffer = new Color32[width * height];
            }
            else
            {
                // Clear the light grid and clear the reusable BFS queue
                Array.Clear(_lightGrid, 0, _lightGrid.Length);
            }

            _bfsQueue.Clear();

            // Step 2: Seed daylight sources
            // A tile is a surface daylight source if there is air in the foreground
            // AND no background tile behind it.
            for (int localX = 0; localX < width; localX++)
            {
                for (int localY = 0; localY < height; localY++)
                {
                    int worldX = inflatedBounds.x + localX;
                    int worldY = inflatedBounds.y + localY;

                    ushort fgTileId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.ForegroundTilemap);
                    ushort bgTileId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.BackgroundTilemap);

                    // For now this just looks for daylight sources not torches
                    if (fgTileId == GameDataRegistry.INVALID_ID && bgTileId == GameDataRegistry.INVALID_ID)
                    {
                        _lightGrid[localX, localY] = _fullBrightnessInterpretation;
                        _bfsQueue.Enqueue(new Vector2Int(localX, localY));
                    }
                }
            }

            // Step 3: Queue-based BFS Flood Fill Propagation
            // Moves in the 4 cardinal directions (Up, Down, Left, Right)
            Vector2Int[] directions = new Vector2Int[]
            {
                new(0, 1),   // Up
                new(0, -1),  // Down
                new(-1, 0),  // Left
                new(1, 0)    // Right
            };

            while (_bfsQueue.Count > 0)
            {
                Vector2Int curr = _bfsQueue.Dequeue();
                float currLight = _lightGrid[curr.x, curr.y];

                for (int i = 0; i < 4; i++)
                {
                    int nextX = curr.x + directions[i].x;
                    int nextY = curr.y + directions[i].y;

                    // Bounds check within the viewport grid
                    if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
                    {
                        int neighborWorldX = inflatedBounds.x + nextX;
                        int neighborWorldY = inflatedBounds.y + nextY;

                        ushort fgTileId = _worldDataStore.GetTileId(neighborWorldX, neighborWorldY, WorldTm.ForegroundTilemap);
                        ushort bgTileId = _worldDataStore.GetTileId(neighborWorldX, neighborWorldY, WorldTm.BackgroundTilemap);

                        // Select attenuation value based on the neighbor tile's layers:
                        // - Solid foreground tile: subtract 1.0
                        // - Background-only tile: subtract 0.5
                        // - Air-only tile (empty): subtract 0.5 (or air attenuation)
                        float attenuation;
                        if (fgTileId != GameDataRegistry.INVALID_ID)
                        {
                            attenuation = _solidForegroundAttenuation;
                        }
                        else if (bgTileId != GameDataRegistry.INVALID_ID)
                        {
                            attenuation = _backgroundOnlyAttenuation;
                        }
                        else
                        {
                            attenuation = _airOnlyAttenuation;
                        }

                        float newLight = currLight - attenuation;

                        // Only propagate if the light exceeds 0 AND improves upon the neighbor's current light level
                        if (newLight > 0 && newLight > _lightGrid[nextX, nextY])
                        {
                            _lightGrid[nextX, nextY] = newLight;
                            _bfsQueue.Enqueue(new Vector2Int(nextX, nextY));
                        }
                    }
                }
            }

            // Step 4: Map grid light values to greyscale pixels for the Texture2D
            // 0 brightness maps to pure black (0,0,0) and _fullBrightnessInterpretation maps to white (255,255,255)
            // Ensure texture coordinates correspond to the Flat buffer: index = y * width + x
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float lightVal = _lightGrid[x, y];
                    float normalized = Mathf.Clamp01(lightVal / _fullBrightnessInterpretation);
                    byte grayscale = (byte)Mathf.RoundToInt(normalized * 255f);

                    int bufferIdx = y * width + x;
                    _colorBuffer[bufferIdx] = new Color32(grayscale, grayscale, grayscale, 255);
                }
            }

            // Ensure filter mode stays in sync if modified in the Inspector at runtime
            if (_lightmapTexture.filterMode != _lightmapFilterMode)
            {
                _lightmapTexture.filterMode = _lightmapFilterMode;
            }

            _lightmapTexture.SetPixels32(_colorBuffer);
            _lightmapTexture.Apply();

            _lightmapOverlay.texture = _lightmapTexture;

            // Apply correct material depending on blending toggle
            if (_enableMultiplyBlending)
            {
                if (_multiplyMaterial == null)
                {
                    InitializeMultiplyMaterial();
                }
                _lightmapOverlay.material = _multiplyMaterial;
            }
            else
            {
                _lightmapOverlay.material = null; // Renders raw grayscale
            }

            // Step 5: Update the Canvas Overlay Transform bounds in world units
            UpdateOverlayRectTf(inflatedBounds);
        }

        /// <summary>
        /// Instantiates the multiply blend material dynamically if none was provided.
        /// </summary>
        private void InitializeMultiplyMaterial()
        {
            if (_multiplyMaterial != null) return;

            Shader multiplyShader = Shader.Find("UI/MultiplyBlend");
            if (multiplyShader != null)
            {
                _multiplyMaterial = new Material(multiplyShader);
            }
            else
            {
                Debug.LogWarning("Multiply blend shader 'UI/MultiplyBlend' not found! Make sure MultiplyUI.shader is imported and compiled.");
            }
        }

        /// <summary>
        /// Fits, scales, and positions the Canvas RawImage overlay to exactly match the world position
        /// boundaries of the camera frustum's visible tiles.
        /// </summary>
        private void UpdateOverlayRectTf(RectInt currentVisibleTileBounds)
        {
            Vector2Int minWorldPos = currentVisibleTileBounds.min;
            Vector2Int maxWorldPos = currentVisibleTileBounds.max;

            // Calculate precise center in floating-point world units (recovers 0.5 unit accuracy vs integer division)
            Vector2 centerWorldPos = new Vector2(minWorldPos.x + maxWorldPos.x, minWorldPos.y + maxWorldPos.y) * 0.5f;
            Vector2 sizeWorld = new(maxWorldPos.x - minWorldPos.x, maxWorldPos.y - minWorldPos.y);

            // Translate bounds directly onto the Canvas Overlay RectTransform
            _lightmapOverlay.rectTransform.position = centerWorldPos; 
            _lightmapOverlay.rectTransform.sizeDelta = sizeWorld;      
            _lightmapOverlay.rectTransform.localScale = Vector3.one;   
        }
    }
}
