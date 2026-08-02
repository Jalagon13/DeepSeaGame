using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepSeaGame
{
    public class FlashlightController : MonoBehaviour
    {
        /// <summary>Fired when the flashlight is toggled on/off or the cone direction changes significantly.</summary>
        public event Action OnFlashlightStateChanged;

        [Header("Flashlight Settings")]
        [Tooltip("Half-angle of the cone in degrees. Total cone = 2x this value.")]
        [SerializeField] private float _coneHalfAngle = 30f;

        [Tooltip("Brightness value seeded at the player tile (analogous to _fullBrightnessInterpretation for ambient light).")]
        [SerializeField] private float _flashlightIntensity = 12f;

        [Tooltip("Maximum range of the flashlight in tiles. Light will not propagate beyond this distance from the player.")]
        [SerializeField] private int _flashlightRange = 12;

        [Tooltip("Minimum mouse movement angle (in degrees) that triggers a lightmap recalculation. Prevents excessive CPU usage.")]
        [SerializeField] private float _recalcAngleThreshold = 2f;

        [Tooltip("Minimum player movement distance (in world units) that triggers a lightmap recalculation while the flashlight is on.")]
        [SerializeField] private float _recalcPositionThreshold = 0.15f;

        private bool _isFlashlightOn;
        public bool IsFlashlightOn => _isFlashlightOn;
        
        private Vector2 _lastDirection;
        private Vector2 _lastPlayerPosition;

        public Vector2 CenterOfPlayerPosition => Player.Instance.PlayerCollider.bounds.center;
        public Vector2Int PlayerCenterTilePosition => Vector2Int.FloorToInt(CenterOfPlayerPosition);
        public float ConeHalfAngle => _coneHalfAngle;
        public float FlashlightIntensity => _flashlightIntensity;
        public int FlashlightRange => _flashlightRange;

        public Vector2 ConeDirection
        {
            get
            {
                Vector2 dir = GameManager.MouseWorldPosition - CenterOfPlayerPosition;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    // Fallback: if mouse is right on top of player, point right
                    return Vector2.right;
                }
                return dir.normalized;
            }
        }

        private void Start()
        {
            GameInput.Instance.OnToggleFlashlight += GameInput_OnToggleFlashlight;
            _lastDirection = ConeDirection;
            _lastPlayerPosition = CenterOfPlayerPosition;
        }

        private void OnDestroy()
        {
            GameInput.Instance.OnToggleFlashlight -= GameInput_OnToggleFlashlight;
        }

        private void Update()
        {
            // When flashlight is on, check if the cone direction or player position has changed enough to warrant a recalculation
            if (!_isFlashlightOn) return;

            Vector2 currentDir = ConeDirection;
            float angleDelta = Vector2.Angle(_lastDirection, currentDir);
            Vector2 currentPlayerPosition = CenterOfPlayerPosition;
            float distanceDelta = Vector2.Distance(_lastPlayerPosition, currentPlayerPosition);

            if (angleDelta >= _recalcAngleThreshold || distanceDelta >= _recalcPositionThreshold)
            {
                _lastDirection = currentDir;
                _lastPlayerPosition = currentPlayerPosition;
                OnFlashlightStateChanged?.Invoke();
            }
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

            // Reset the stored direction and player position so the first frame after toggling on always fires a recalculation
            _lastDirection = ConeDirection;
            _lastPlayerPosition = CenterOfPlayerPosition;

            // Fire event so LightmapManager knows to recalculate
            OnFlashlightStateChanged?.Invoke();
        }
    }
}