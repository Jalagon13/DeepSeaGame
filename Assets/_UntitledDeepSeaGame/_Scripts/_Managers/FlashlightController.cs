using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    /// <summary>
    /// Manages the player's flashlight: a cone of light projected from the player toward the mouse cursor.
    /// The cone is ray-marched through the tile grid, stopping at solid walls (but lighting the wall face).
    /// Writes directly into LightmapManager.LightGrid via the OnBeforeApplyLightmap event.
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        [Header("Flashlight Config")]
        [SerializeField]
        [Tooltip("Half-angle of the flashlight cone in degrees. Total spread is 2x this value.")]
        private float _coneHalfAngle = 30f;

        [SerializeField]
        [Tooltip("Maximum distance in tiles the flashlight can reach.")]
        private float _range = 12f;

        [SerializeField]
        [Tooltip("Brightness of the flashlight at the player origin, matching LightmapManager's fullBrightnessInterpretation scale.")]
        private float _brightness = 15f;

        [SerializeField]
        [Tooltip("How much light value is lost per tile travelled. Higher = shorter, dimmer beam.")]
        private float _falloffPerTile = 0.8f;

        [SerializeField]
        [Tooltip("Number of rays spanning the cone. More rays = smoother edges but more CPU.")]
        private int _rayCount = 24;

        [SerializeField]
        [Tooltip("Key to toggle the flashlight on/off.")]
        private Key _toggleKey = Key.F;

        [Header("Debug")]
        [SerializeField]
        private bool _flashlightOn = true;

        [SerializeField]
        private bool _drawDebugRays = false;

        private bool _subscribed;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private float _timeSinceLastFlashlightUpdate;

        private void Update()
        {
            // Check for toggle input
            if (Keyboard.current[_toggleKey].wasPressedThisFrame)
            {
                _flashlightOn = !_flashlightOn;
                
                // When toggling on, immediately request a flashlight update
                if (_flashlightOn && LightmapManager.Instance != null)
                {
                    LightmapManager.Instance.RequestFlashlightUpdate();
                }
            }

            // Per-frame flashlight update: only when on and at a capped rate
            // to avoid hammering the texture upload every frame for no visual gain.
            if (_flashlightOn && LightmapManager.Instance != null)
            {
                _timeSinceLastFlashlightUpdate += Time.unscaledDeltaTime;
                
                // Throttle to ~30 updates/sec (mouse movement doesn't need 60fps for pixel tiles)
                if (_timeSinceLastFlashlightUpdate >= 0.033f)
                {
                    _timeSinceLastFlashlightUpdate = 0f;
                    LightmapManager.Instance.RequestFlashlightUpdate();
                }
            }
            else
            {
                _timeSinceLastFlashlightUpdate = 0f;
            }
        }

        private void Subscribe()
        {
            if (_subscribed || LightmapManager.Instance == null) return;

            LightmapManager.Instance.OnBeforeApplyLightmap += ApplyFlashlightCone;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || LightmapManager.Instance == null) return;

            LightmapManager.Instance.OnBeforeApplyLightmap -= ApplyFlashlightCone;
            _subscribed = false;
        }

        /// <summary>
        /// Called by LightmapManager just before it uploads the light grid to the texture.
        /// Ray-marches a cone of rays from the player toward the mouse cursor and writes
        /// flashlight light values into the grid using Mathf.Max (additive blending with ambient).
        /// </summary>
        private void ApplyFlashlightCone(RectInt inflatedBounds)
        {
            if (!_flashlightOn) return;

            LightmapManager lm = LightmapManager.Instance;
            if (lm == null) return;

            float[,] lightGrid = lm.LightGrid;
            if (lightGrid == null) return;

            // 1. Get player tile position
            Player player = Player.Instance;
            if (player == null) return;

            Vector2 playerWorldPos = player.PlayerCenter;
            Vector2Int playerTile = new Vector2Int(
                Mathf.FloorToInt(playerWorldPos.x),
                Mathf.FloorToInt(playerWorldPos.y)
            );

            // 2. Get cursor world position and compute cone center direction
            Vector2 cursorWorldPos = GameManager.MouseWorldPosition;
            Vector2 coneCenterDir = (cursorWorldPos - playerWorldPos).normalized;

            // If player and cursor are on the same tile, no meaningful direction
            if (coneCenterDir.sqrMagnitude < 0.0001f) return;

            // 3. Convert player tile to grid-local coords
            Vector2Int localPlayer = new Vector2Int(
                playerTile.x - inflatedBounds.x,
                playerTile.y - inflatedBounds.y
            );

            // If player is outside the inflated bounds, skip
            int gridWidth = lightGrid.GetLength(0);
            int gridHeight = lightGrid.GetLength(1);
            if (localPlayer.x < 0 || localPlayer.x >= gridWidth ||
                localPlayer.y < 0 || localPlayer.y >= gridHeight)
                return;

            // 4. Compute ray fan
            WorldDataStore worldData = null;
            if (WorldManager.Instance != null)
                worldData = WorldManager.Instance.WorldDataStore;

            float halfAngleRad = _coneHalfAngle * Mathf.Deg2Rad;
            float angleStep = (2f * halfAngleRad) / Mathf.Max(1, _rayCount - 1);
            float baseAngle = Mathf.Atan2(coneCenterDir.y, coneCenterDir.x) - halfAngleRad;

            for (int i = 0; i < _rayCount; i++)
            {
                float angle = baseAngle + angleStep * i;
                Vector2 rayDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // March this ray outward from the player
                RayMarch(localPlayer, playerTile, rayDir, inflatedBounds, lightGrid, worldData);
            }
        }

        /// <summary>
        /// Marches a single ray outward from the player through the tile grid.
        /// Writes light values into the grid for each visited tile. Stops when it hits
        /// a solid wall (after lighting the wall tile), exceeds range, or leaves the grid.
        /// </summary>
        private void RayMarch(
            Vector2Int localOrigin,
            Vector2Int worldOrigin,
            Vector2 direction,
            RectInt inflatedBounds,
            float[,] lightGrid,
            WorldDataStore worldData)
        {
            float lightValue = _brightness;
            int maxSteps = Mathf.CeilToInt(_range);

            // Use a simple step-based march. For each integer step along the ray,
            // we snap to the nearest tile coordinate and check it.
            // This is a grid-aligned DDA-like approach: step in units of tile size.
            for (int step = 0; step < maxSteps; step++)
            {
                // Compute world tile position at this step
                // We step in increments of 0.75 tile sizes to avoid skipping thin walls
                // while still covering tiles at each integer position
                float t = step * 0.75f + 0.5f; // start a half-tile away from origin
                Vector2 worldPos = new Vector2(
                    worldOrigin.x + direction.x * t,
                    worldOrigin.y + direction.y * t
                );

                Vector2Int worldTile = new Vector2Int(
                    Mathf.RoundToInt(worldPos.x),
                    Mathf.RoundToInt(worldPos.y)
                );

                // Convert to grid-local coords
                Vector2Int localTile = new Vector2Int(
                    worldTile.x - inflatedBounds.x,
                    worldTile.y - inflatedBounds.y
                );

                // Check bounds
                if (localTile.x < 0 || localTile.x >= lightGrid.GetLength(0) ||
                    localTile.y < 0 || localTile.y >= lightGrid.GetLength(1))
                    break;

                // Write light value (max with existing so flashlight adds to ambient)
                lightGrid[localTile.x, localTile.y] = Mathf.Max(lightGrid[localTile.x, localTile.y], lightValue);

                // Debug visualization
                if (_drawDebugRays)
                {
                    Debug.DrawLine(
                        new Vector3(worldTile.x, worldTile.y, 0f),
                        new Vector3(worldTile.x, worldTile.y, 0f) + Vector3.up * 0.3f,
                        Color.yellow,
                        0.02f,
                        false
                    );
                }

                // Check for solid wall — stop propagation but the wall is already lit
                if (worldData != null)
                {
                    ushort fgId = worldData.GetTileId(worldTile.x, worldTile.y, WorldTm.ForegroundTilemap);
                    if (fgId != GameDataRegistry.INVALID_ID)
                    {
                        TileSO fgTile = GameDataRegistry.Instance.GetTileSOFromTileId(fgId);
                        if (fgTile != null && fgTile.IsSolid)
                        {
                            // Wall is lit. Stop this ray.
                            break;
                        }
                    }
                }

                // Apply attenuation for next step
                lightValue -= _falloffPerTile;
                if (lightValue <= 0f) break;
            }
        }
    }
}