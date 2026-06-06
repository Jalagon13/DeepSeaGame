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

        [Tooltip("The number of tiles light can traverse from its source before attenuation/dimming begins.")]
        [SerializeField]
        private int _tileAmountBeforeAttenuationBegins = 2;

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
        private RectInt _currentVisibleTileBounds;
        private RectInt _currentInflatedBounds;
        private float[,] _lightGrid;
        private int[,] _distGrid;
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

            // Subscribe to tile changes in the world data store to update lighting when blocks are broken/placed
            if (WorldManager.Instance.IsWorldReady)
            {
                SubscribeToTileChanges();
            }
            else
            {
                WorldManager.Instance.OnWorldReady += SubscribeToTileChanges;
            }
        }
        
        private void OnDestroy() 
        {
            PlayerCamera.OnVisibleTileBoundsChanged -= UpdateLightmap;
            WorldManager.Instance.OnWorldReady -= SubscribeToTileChanges;

            if (_worldDataStore != null)
            {
                _worldDataStore.TileChanged -= HandleTileChanged;
            }

            // Clean up resources to prevent memory leaks
            if (_lightmapTexture != null)
            {
                Destroy(_lightmapTexture);
            }
        }

        private void SubscribeToTileChanges()
        {
            if (_worldDataStore == null && WorldManager.Instance != null)
            {
                _worldDataStore = WorldManager.Instance.WorldDataStore;
            }

            if (_worldDataStore != null)
            {
                _worldDataStore.TileChanged -= HandleTileChanged;
                _worldDataStore.TileChanged += HandleTileChanged;
            }
        }

        private void HandleTileChanged(Vector2Int tilePosition, ushort previousTileId, ushort newTileId, WorldTm targetMap)
        {
            // Only trigger recalculation if the modified tile falls inside the active inflated calculations boundary
            if (_gridWidth > 0 && _gridHeight > 0 && _currentInflatedBounds.Contains(tilePosition))
            {
                UpdateLightmap(_currentVisibleTileBounds);
            }
        }

        /// <summary>
        /// Main lightmap recalculation entry point. Orchestrates the full pipeline:
        /// bounds inflation → buffer preparation → daylight seeding → BFS propagation → texture upload.
        /// </summary>
        /// <param name="currentVisibleTileBounds">The RectInt defining the current camera frustum in tile coords.</param>
        private void UpdateLightmap(RectInt currentVisibleTileBounds)
        {
            if (WorldManager.Instance == null || !WorldManager.Instance.IsWorldReady) return;

            if (_worldDataStore == null)
            {
                _worldDataStore = WorldManager.Instance.WorldDataStore;
                if (_worldDataStore == null) return;
            }

            _currentVisibleTileBounds = currentVisibleTileBounds;

            if (!TryInflateBounds(currentVisibleTileBounds, out RectInt inflatedBounds)) return;

            PrepareLightmap(inflatedBounds.width, inflatedBounds.height);
            SeedLightSources(inflatedBounds);
            RunBFSPropagation(inflatedBounds);
            ApplyLightmapToOverlay(inflatedBounds);
        }

        /// <summary>
        /// Expands the camera frustum bounds outward by the configured padding to prevent lighting
        /// pop-in on screen edges. Returns false if the resulting bounds are degenerate.
        /// </summary>
        private bool TryInflateBounds(RectInt visibleBounds, out RectInt inflatedBounds)
        {
            int minX = visibleBounds.xMin - _extraLightmapPadding;
            int minY = visibleBounds.yMin - _extraLightmapPadding;
            int maxX = visibleBounds.xMax + _extraLightmapPadding;
            int maxY = visibleBounds.yMax + _extraLightmapPadding;

            inflatedBounds = new RectInt(minX, minY, maxX - minX, maxY - minY);
            _currentInflatedBounds = inflatedBounds;

            return inflatedBounds.width > 0 && inflatedBounds.height > 0;
        }

        /// <summary>
        /// Allocates (or reuses) the light grid, color buffer, and Texture2D for the given dimensions.
        /// Allocation only occurs when dimensions change to avoid GC spikes each frame.
        /// </summary>
        private void PrepareLightmap(int width, int height)
        {
            if (_lightGrid == null || _gridWidth != width || _gridHeight != height)
            {
                _lightGrid = new float[width, height];
                _distGrid = new int[width, height];
                _gridWidth = width;
                _gridHeight = height;

                if (_lightmapTexture != null) Destroy(_lightmapTexture);

                _lightmapTexture = new Texture2D(width, height, TextureFormat.RGB24, false)
                {
                    filterMode = _lightmapFilterMode,
                    wrapMode = TextureWrapMode.Clamp
                };

                _colorBuffer = new Color32[width * height];
            }
            else
            {
                // Reuse existing buffers — just zero out the light grid
                Array.Clear(_lightGrid, 0, _lightGrid.Length);
            }

            // Initialize all distances to int.MaxValue
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _distGrid[x, y] = int.MaxValue;
                }
            }

            _bfsQueue.Clear();
        }

        /// <summary>
        /// Scans the inflated grid and seeds any tile that has no foreground AND no background as a
        /// full-brightness daylight source (value = 15), enqueuing it for BFS propagation.
        /// </summary>
        private void SeedLightSources(RectInt inflatedBounds)
        {
            int width  = inflatedBounds.width;
            int height = inflatedBounds.height;

            for (int localX = 0; localX < width; localX++)
            {
                for (int localY = 0; localY < height; localY++)
                {
                    int worldX = inflatedBounds.x + localX;
                    int worldY = inflatedBounds.y + localY;

                    ushort fgId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.ForegroundTilemap);
                    ushort bgId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.BackgroundTilemap);
                    TileSO fgTile = GameDataRegistry.Instance.GetTileSOFromTileId(fgId);

                    bool isOpenSky = fgId == GameDataRegistry.INVALID_ID && bgId == GameDataRegistry.INVALID_ID && 
                                     worldY > WorldManager.Instance.WorldGenerator.WorldGenerationData.UndergroundMaxYLevel;
                                    
                    if (isOpenSky)
                    {
                        _lightGrid[localX, localY] = _fullBrightnessInterpretation;
                        _distGrid[localX, localY] = 0;
                        _bfsQueue.Enqueue(new Vector2Int(localX, localY));
                        continue;
                    }
                    else if(fgId == GameDataRegistry.INVALID_ID)
                    {
                        continue;
                    }

                    if (fgTile.LightValue > 0)
                    {
                        _lightGrid[localX, localY] = fgTile.LightValue;
                        _distGrid[localX, localY] = 0;
                        _bfsQueue.Enqueue(new Vector2Int(localX, localY));
                    }
                }
            }
        }

        /// <summary>
        /// Iterative queue-based BFS that propagates light outward from all seeded sources simultaneously.
        /// Each step applies per-tile attenuation based on whether the neighbor is solid, background-only, or open air.
        /// A neighbor is only enqueued if the new brightness strictly improves on its current stored value.
        /// </summary>
        private void RunBFSPropagation(RectInt inflatedBounds)
        {
            int width  = inflatedBounds.width;
            int height = inflatedBounds.height;

            // Static cardinal directions: Up, Down, Left, Right
            Vector2Int[] directions = { new(0, 1), new(0, -1), new(-1, 0), new(1, 0) };

            while (_bfsQueue.Count > 0)
            {
                Vector2Int curr = _bfsQueue.Dequeue();
                float currLight = _lightGrid[curr.x, curr.y];
                int currDist = _distGrid[curr.x, curr.y];

                foreach (Vector2Int dir in directions)
                {
                    int nextX = curr.x + dir.x;
                    int nextY = curr.y + dir.y;

                    if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height) continue;

                    int nextDist = currDist + 1;

                    int worldX = inflatedBounds.x + nextX;
                    int worldY = inflatedBounds.y + nextY;

                    ushort fgId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.ForegroundTilemap);
                    ushort bgId = _worldDataStore.GetTileId(worldX, worldY, WorldTm.BackgroundTilemap);

                    float attenuation = 0f;
                    if (nextDist > _tileAmountBeforeAttenuationBegins)
                    {
                        attenuation = GetTileAttenuation(fgId, bgId);
                    }

                    float newLight = currLight - attenuation;

                    if (newLight > 0f)
                    {
                        bool shouldUpdate = false;
                        if (newLight > _lightGrid[nextX, nextY])
                        {
                            shouldUpdate = true;
                        }
                        else if (newLight == _lightGrid[nextX, nextY] && nextDist < _distGrid[nextX, nextY])
                        {
                            shouldUpdate = true;
                        }

                        if (shouldUpdate)
                        {
                            _lightGrid[nextX, nextY] = newLight;
                            _distGrid[nextX, nextY] = nextDist;
                            _bfsQueue.Enqueue(new Vector2Int(nextX, nextY));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the correct light attenuation value for a given tile based on its layer state.
        /// </summary>
        private float GetTileAttenuation(ushort fgId, ushort bgId)
        {
            if (fgId != GameDataRegistry.INVALID_ID) return _solidForegroundAttenuation;
            if (bgId != GameDataRegistry.INVALID_ID) return _backgroundOnlyAttenuation;
            return _airOnlyAttenuation;
        }

        /// <summary>
        /// Converts the finished float light grid into a grayscale Texture2D and uploads it
        /// to the RawImage overlay, applying the multiply material if blending is enabled.
        /// </summary>
        private void ApplyLightmapToOverlay(RectInt inflatedBounds)
        {
            int width  = inflatedBounds.width;
            int height = inflatedBounds.height;

            // Map each light value [0, 15] to a grayscale byte [0, 255]
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalized = Mathf.Clamp01(_lightGrid[x, y] / _fullBrightnessInterpretation);
                    byte  grayscale  = (byte)Mathf.RoundToInt(normalized * 255f);
                    _colorBuffer[y * width + x] = new Color32(grayscale, grayscale, grayscale, 255);
                }
            }

            // Sync filter mode if tweaked in the Inspector at runtime
            if (_lightmapTexture.filterMode != _lightmapFilterMode)
                _lightmapTexture.filterMode = _lightmapFilterMode;

            _lightmapTexture.SetPixels32(_colorBuffer);
            _lightmapTexture.Apply();

            _lightmapOverlay.texture = _lightmapTexture;

            // Assign multiply material or fall back to raw grayscale
            if (_enableMultiplyBlending)
            {
                if (_multiplyMaterial == null) InitializeMultiplyMaterial();
                _lightmapOverlay.material = _multiplyMaterial;
            }
            else
            {
                _lightmapOverlay.material = null;
            }

            UpdateOverlayRectTf(inflatedBounds);
        }

        /// <summary>
        /// Instantiates the multiply blend material dynamically if none was provided in the Inspector.
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
                Debug.LogWarning("LightmapManager: Shader 'UI/MultiplyBlend' not found. Make sure MultiplyUI.shader is imported.");
            }
        }

        /// <summary>
        /// Positions and sizes the Canvas RawImage overlay to exactly cover the inflated tile bounds in world space.
        /// </summary>
        private void UpdateOverlayRectTf(RectInt bounds)
        {
            Vector2 center = new Vector2(bounds.xMin + bounds.xMax, bounds.yMin + bounds.yMax) * 0.5f;
            Vector2 size   = new Vector2(bounds.width, bounds.height);

            _lightmapOverlay.rectTransform.position   = center;
            _lightmapOverlay.rectTransform.sizeDelta  = size;
            _lightmapOverlay.rectTransform.localScale = Vector3.one;
        }
    }
}
