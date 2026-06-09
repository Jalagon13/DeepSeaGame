using System;
using FMODUnity;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UntitledDeepSeaGame
{
    public class PlayerCamera : NetworkBehaviour
    {
        public static PlayerCamera Instance { get; private set; }
        public static event Action<RectInt> OnVisibleTileBoundsChanged;

        [field: SerializeField, Tooltip("How much padding, from the min and max points of the camera bounds to give to cover the whole frustum for the lightmap")] 
        public float MinMaxOffsetPadding { get; private set; }
        
        [SerializeField] private PolygonCollider2D _boundaryCollider; // For the cinemachine camera
        [SerializeField] private EdgeCollider2D _edgeCollider; // For the player

        public RectInt CurrentVisibleTileBounds { get; private set; }

        private BoxCollider2D _cameraFrustumCollider;
        private Camera _mainCamera;
        private CinemachineCamera _cinemachineCam;
        private NetworkObject _playerObject;
        private CinemachineConfiner2D _confiner;


        private void Awake()
        {
            Instance = this;
            _cameraFrustumCollider = GetComponent<BoxCollider2D>();
            _confiner = GetComponent<CinemachineConfiner2D>();
            _cinemachineCam = GetComponent<CinemachineCamera>();
            _cinemachineCam.enabled = false;
            
            _mainCamera = Camera.main;

            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback += RegisterCameraToPlayer;
            }
        }

        // NTFS: Change this dynamically when camera is widened or narrowed
        private void Start()
        {
            float verticalSize = _mainCamera.orthographicSize * 2;
            float horizontalSize = verticalSize * _mainCamera.aspect;
            _cameraFrustumCollider.size = new Vector2(horizontalSize, verticalSize);
            _cameraFrustumCollider.offset = Vector2.zero;
        }

        public override void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (NetworkManager != null)
            {
                NetworkManager.OnClientConnectedCallback -= RegisterCameraToPlayer;
            }
        }
        
        private void Update()
        {
            if (_playerObject == null) return;
            SetListenerToPlayer();

            // Update the frustum collider in case the camera size has changed
            float verticalSize = _mainCamera.orthographicSize * 2;
            float horizontalSize = verticalSize * _mainCamera.aspect;
            _cameraFrustumCollider.size = new Vector2(horizontalSize, verticalSize);
            _cameraFrustumCollider.offset = Vector2.zero;
        }

        private void LateUpdate()
        {
            // Update Visibile Tile Bounds
            if (_mainCamera == null)
            {
                return;
            }

            int padding = Mathf.CeilToInt(MinMaxOffsetPadding);
            Vector3 bottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 topRight = _mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            int minX = Mathf.FloorToInt(bottomLeft.x) - padding;
            int minY = Mathf.FloorToInt(bottomLeft.y) - padding;
            int maxX = Mathf.CeilToInt(topRight.x) + padding;
            int maxY = Mathf.CeilToInt(topRight.y) + padding;
            RectInt visibleBounds = new RectInt(minX, minY, Mathf.Max(0, maxX - minX), Mathf.Max(0, maxY - minY));

            if (visibleBounds == CurrentVisibleTileBounds)
            {
                return;
            }

            CurrentVisibleTileBounds = visibleBounds;
            OnVisibleTileBoundsChanged?.Invoke(CurrentVisibleTileBounds);
        }

        private void RegisterCameraToPlayer(ulong clientId)
        {
            if (NetworkManager.LocalClientId != clientId) return;

            _playerObject = NetworkManager.ConnectedClients[clientId].PlayerObject;
            _cinemachineCam.Follow = _playerObject.transform;
            _cinemachineCam.enabled = true;
            
            int worldWidth = WorldManager.Instance.WorldGenerator.WorldGenerationData.WorldWidth;
            int worldHeight = WorldManager.Instance.WorldGenerator.WorldGenerationData.WorldHeight;

            _boundaryCollider.points = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(worldWidth, 0),
                new Vector2(worldWidth, worldHeight),
                new Vector2(0, worldHeight)
            };

            _edgeCollider.points = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(worldWidth, 0),
                new Vector2(worldWidth, worldHeight),
                new Vector2(0, worldHeight)
            };

            _confiner.InvalidateBoundingShapeCache();

            SetListenerToPlayer();
        }

        private void SetListenerToPlayer()
        {
            var attributes = new FMOD.ATTRIBUTES_3D
            {
                position = new FMOD.VECTOR
                {
                    x = _playerObject.transform.position.x,
                    y = _playerObject.transform.position.y,
                    z = _playerObject.transform.position.z
                }
            };
            RuntimeManager.StudioSystem.setListenerAttributes(0, attributes);
        }
    }
}
