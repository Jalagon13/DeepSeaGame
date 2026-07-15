using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    /// <summary>
    /// Controls the flashlight state and exposes data for the LightmapManager
    /// to use when performing the cone-constrained BFS light propagation.
    /// Fires events when the flashlight toggles or the cone direction changes
    /// so the LightmapManager knows to recalculate.
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        /// <summary>Fired when the flashlight is toggled on/off or the cone direction changes significantly.</summary>
        public static event Action OnFlashlightStateChanged;

        [Header("Flashlight Settings")]
        [Tooltip("Half-angle of the cone in degrees. Total cone = 2x this value.")]
        [SerializeField] private float _coneHalfAngle = 30f;

        [Tooltip("Brightness value seeded at the player tile (analogous to _fullBrightnessInterpretation for ambient light).")]
        [SerializeField] private float _flashlightIntensity = 12f;

        [Tooltip("Minimum mouse movement angle (in degrees) that triggers a lightmap recalculation. Prevents excessive CPU usage.")]
        [SerializeField] private float _recalcAngleThreshold = 2f;

        private bool _isFlashlightOn;
        private Vector2 _lastDirection;

        /// <summary>Whether the flashlight is currently active.</summary>
        public bool IsFlashlightOn => _isFlashlightOn;

        /// <summary>The player's position in world coordinates.</summary>
        public Vector2 PlayerWorldPosition => transform.position;

        /// <summary>Tile-aligned integer position of the player.</summary>
        public Vector2Int PlayerTilePosition => Vector2Int.FloorToInt(transform.position);

        /// <summary>
        /// Normalised direction from the player toward the mouse cursor in world space.
        /// This becomes the central axis of the flashlight cone.
        /// </summary>
        public Vector2 ConeDirection
        {
            get
            {
                Vector2 dir = GameManager.MouseWorldPosition - PlayerWorldPosition;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    // Fallback: if mouse is right on top of player, point right
                    return Vector2.right;
                }
                return dir.normalized;
            }
        }

        /// <summary>Half-angle of the cone in degrees.</summary>
        public float ConeHalfAngle => _coneHalfAngle;

        /// <summary>Brightness seeded at the flashlight origin tile.</summary>
        public float FlashlightIntensity => _flashlightIntensity;

        private void Start()
        {
            GameInput.Instance.OnToggleFlashlight += GameInput_OnToggleFlashlight;
            _lastDirection = ConeDirection;
        }

        private void Update()
        {
            // When flashlight is on, check if the cone direction has changed enough to warrant a recalculation
            if (!_isFlashlightOn) return;

            Vector2 currentDir = ConeDirection;
            float angleDelta = Vector2.Angle(_lastDirection, currentDir);

            if (angleDelta >= _recalcAngleThreshold)
            {
                _lastDirection = currentDir;
                OnFlashlightStateChanged?.Invoke();
            }
        }

        private void OnDestroy()
        {
            GameInput.Instance.OnToggleFlashlight -= GameInput_OnToggleFlashlight;
        }

        private void GameInput_OnToggleFlashlight(object sender, InputAction.CallbackContext e)
        {
            if (e.started)
            {
                ToggleFlashlight();
            }
        }

        private void ToggleFlashlight()
        {
            _isFlashlightOn = !_isFlashlightOn;
            Debug.Log($"Flashlight Toggle: {(_isFlashlightOn ? "ON" : "OFF")}");

            // Reset the stored direction so the first frame after toggling on always fires a recalculation
            _lastDirection = ConeDirection;

            // Fire event so LightmapManager knows to recalculate
            OnFlashlightStateChanged?.Invoke();
        }
    }
}