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

        private void TryToPlaceTile()
        {
            if (!TryGetPlaceableTile(out Vector2Int tilePosition, out TileSO tileSO))
            {
                return;
            }

            if(tileSO.IsMultiTile)
            {
                WorldManager.Instance.WorldDataStore.SetMultiTile(tilePosition.x, tilePosition.y, tileSO);
            }
            else
            {
                WorldManager.Instance.WorldDataStore.SetTileId(tilePosition.x, tilePosition.y, GameDataRegistry.Instance.GetTileIdFromTileSO(tileSO));
            }
            
            InventoryManager.Instance.SubtractOneFromHotbarSelectedSlot();
        }

        private bool TryGetPlaceableTile(out Vector2Int tilePosition, out TileSO tileSO)
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
            
            if(tileSO.IsMultiTile)
            {
                Vector2Int size = tileSO.Size;
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        int checkX = tilePosition.x + x;
                        int checkY = tilePosition.y + y;
                        if (!worldDataStore.IsInBounds(checkX, checkY) || worldDataStore.GetTileId(checkX, checkY) != GameDataRegistry.INVALID_ID)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            return worldDataStore.GetTileId(tilePosition.x, tilePosition.y) == GameDataRegistry.INVALID_ID;
        }

        private bool PlayerWithinPlacingRangeOfMouse()
        {
            return Vector2.Distance(Player.Instance.PlayerCenter, GameManager.MouseWorldPosition) <= _placementRange;
        }
    }
}
