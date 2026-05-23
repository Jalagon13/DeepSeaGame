using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        [SerializeField]
        private float _placementRange = 3f;
        public float PlacementRange => _placementRange;

        private PlacingState _placingState;
        private TileItemSO _currentTileItem;

        public PlacingState PlacingState => _placingState;
        public TileItemSO CurrentTileItem => _currentTileItem;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameInput.Instance.OnPrimaryActionStarted += OnPrimaryActionStarted;
            InventoryManager.Instance.OnSelectedHotbarSlotChanged += OnSelectedHotbarSlotChanged;
        }

        private void OnDestroy()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnPrimaryActionStarted -= OnPrimaryActionStarted;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnSelectedHotbarSlotChanged -= OnSelectedHotbarSlotChanged;
            }
        }

        private void Update()
        {
            if (_placingState == PlacingState.Placing)
            {
                TryToPlaceTile();
            }
        }

        private void TryToPlaceTile()
        {
            if (!TryGetPlaceableTile(out Vector3Int tilePosition, out TileSO tileSO))
            {
                return;
            }

            WorldManager.Instance.WorldDataStore.SetTileId(tilePosition.x, tilePosition.y, GameDataRegistry.Instance.GetTileIdFromTileSO(tileSO));
            InventoryManager.Instance.SubtractOneFromHotbarSelectedSlot();
        }

        private void OnSelectedHotbarSlotChanged(int arg1, InventoryStack stack)
        {
            _currentTileItem = !stack.IsEmpty && stack.Item is TileItemSO tileItemSO ? tileItemSO : null;

            if (_currentTileItem == null)
            {
                _placingState = PlacingState.Idle;
            }
        }

        private void OnPrimaryActionStarted(object sender, InputAction.CallbackContext e)
        {
            if (_currentTileItem == null)
            {
                _placingState = PlacingState.Idle;
                return;
            }

            PlacingState newState = (e.started || e.performed) ? PlacingState.Placing : PlacingState.Idle;
            if (_placingState == newState)
            {
                return;
            }

            _placingState = newState;
        }

        private bool PlayerWithinPlacingRangeOfMouse()
        {
            return Vector2.Distance(Player.Instance.PlayerCenter, GameManager.MouseWorldPosition) <= _placementRange;
        }

        private bool TryGetPlaceableTile(out Vector3Int tilePosition, out TileSO tileSO)
        {
            tilePosition = GameManager.MouseTilePosition;
            tileSO = _currentTileItem == null ? null : _currentTileItem.PlaceableTile;

            if (tileSO == null || WorldManager.Instance?.WorldDataStore == null || !PlayerWithinPlacingRangeOfMouse())
            {
                return false;
            }

            WorldDataStore worldDataStore = WorldManager.Instance.WorldDataStore;
            if (!worldDataStore.IsInBounds(tilePosition.x, tilePosition.y))
            {
                return false;
            }

            return worldDataStore.GetTileId(tilePosition.x, tilePosition.y) == GameDataRegistry.INVALID_ID;
        }
    }
}
