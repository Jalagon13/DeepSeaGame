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
            if (!CanPlaceTile(out Vector2Int tilePosition, out TileSO tileSO))
            {
                return;
            }

            if(tileSO.IsMultiTile)
            {
                WorldManager.Instance.WorldDataStore.SetMultiTile(tilePosition.x, tilePosition.y, tileSO);
            }
            else
            {
                ushort tileId = GameDataRegistry.Instance.GetTileIdFromTileSO(tileSO);
                WorldManager.Instance.WorldDataStore.SetTileId(tilePosition.x, tilePosition.y, tileId, tileSO.TileType);
            }
            
            InventoryManager.Instance.SubtractOneFromHotbarSelectedSlot();
        }

        private bool CanPlaceTile(out Vector2Int tilePosition, out TileSO tileSO)
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
            }
            else
            {
                if (worldDataStore.GetTileId(tilePosition.x, tilePosition.y, tileSO.TileType) != GameDataRegistry.INVALID_ID)
                {
                    return false;
                }
            }

            return HasAdjacentSupport(tilePosition, tileSO.TileType);
        }

        private bool HasAdjacentSupport(Vector2Int position, WorldTm placingMap)
        {
            WorldDataStore dataStore = WorldManager.Instance.WorldDataStore;
            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in neighbors)
            {
                Vector2Int neighbor = position + dir;
                if (!dataStore.IsInBounds(neighbor.x, neighbor.y)) continue;

                // 1. Solid Foreground is a valid support neighbor for both Foreground and Background placement
                ushort fgId = dataStore.GetTileId(neighbor.x, neighbor.y, WorldTm.ForegroundTilemap);
                if (fgId != GameDataRegistry.INVALID_ID)
                {
                    TileSO fgSO = GameDataRegistry.Instance.GetTileSOFromTileId(fgId);
                    if (fgSO != null && !fgSO.IsMultiTile) return true;
                }

                // 2. Existing Background is a valid support neighbor ONLY if we are currently placing a Background tile (Wall)
                if (placingMap == WorldTm.BackgroundTilemap && dataStore.GetTileId(neighbor.x, neighbor.y, WorldTm.BackgroundTilemap) != GameDataRegistry.INVALID_ID)
                {
                    return true;
                }
            }

            return false;
        }

        private bool PlayerWithinPlacingRangeOfMouse()
        {
            return Vector2.Distance(Player.Instance.PlayerCenter, GameManager.MouseWorldPosition) <= _placementRange;
        }
    }
}
