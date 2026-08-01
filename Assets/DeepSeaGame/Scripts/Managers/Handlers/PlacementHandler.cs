using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepSeaGame
{
    public class PlacementHandler : MonoBehaviour, IItemUseHandler
    {
        [SerializeField]
        private float _placementRange = 3f;
        public float PlacementRange => _placementRange;

        [SerializeField]
        private int _lightSourcePlacementAttempts = 8;

        private PlacingState _placingState;
        private TileItemSO _currentTileItem;

        public PlacingState PlacingState => _placingState;
        public TileItemSO CurrentTileItem => _currentTileItem;

        private void Start() 
        {
            GameInput.Instance.OnPlaceLightSource += GameInput_OnPlaceLightSource;    
        }
        
        private void OnDestroy() 
        {
            GameInput.Instance.OnPlaceLightSource -= GameInput_OnPlaceLightSource;
        }

        private void GameInput_OnPlaceLightSource(object sender, InputAction.CallbackContext e)
        {
            // Try to find a light source item in the inventory
            TileItemSO lightSourceItem = null;
            foreach (var stack in InventoryManager.Instance.Slots)
            {
                if (!stack.IsEmpty && stack.Item is TileItemSO tileItem)
                {
                    if (tileItem.PrimaryTile != null && tileItem.PrimaryTile.LightValue > 0)
                    {
                        lightSourceItem = tileItem;
                        break;
                    }
                }
            }

            if (lightSourceItem == null) return;

            TileSO lightTileSO = lightSourceItem.PrimaryTile;
            
            Vector2 playerPos = Player.Instance.PlayerCenter;
            Vector2 mouseWorldPos = GameManager.MouseWorldPosition;

            // If the mouse position is outside the placement range, we need to clamp it to the edge of the placement range
            Vector2 startWorldPos = mouseWorldPos;
            if (Vector2.Distance(playerPos, mouseWorldPos) > _placementRange)
            {
                Vector2 dir = (mouseWorldPos - playerPos).normalized;
                startWorldPos = playerPos + dir * _placementRange;
            }

            Vector2Int startPos = Vector2Int.FloorToInt(startWorldPos);
            
            // Use a breadth-first search (BFS) to find the nearest valid placement position for the light source tile
            Queue<Vector2Int> queue = new();
            HashSet<Vector2Int> visited = new();
            
            queue.Enqueue(startPos);
            visited.Add(startPos);

            int attempts = 0;
            Vector2Int[] neighbors = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };

            while (queue.Count > 0 && attempts < _lightSourcePlacementAttempts)
            {
                Vector2Int currentPos = queue.Dequeue();
                attempts++;

                if (CanPlaceSpecificTile(currentPos, lightTileSO))
                {
                    PlaceSpecificTile(currentPos, lightTileSO);
                    InventoryManager.Instance.RemoveItem(lightSourceItem, 1);
                    return; 
                }

                foreach (var dir in neighbors)
                {
                    Vector2Int neighborPos = currentPos + dir;
                    if (!visited.Contains(neighborPos))
                    {
                        visited.Add(neighborPos);
                        queue.Enqueue(neighborPos);
                    }
                }
            }
        }

        private bool CanPlaceSpecificTile(Vector2Int tilePosition, TileSO tileSO)
        {
            if (tileSO == null || WorldManager.Instance.WorldDataStore == null) return false;
            
            Vector2 tileWorldPos = new(tilePosition.x + 0.5f, tilePosition.y + 0.5f);
            if (Vector2.Distance(Player.Instance.PlayerCenter, tileWorldPos) > _placementRange) return false;

            WorldDataStore worldDataStore = WorldManager.Instance.WorldDataStore;
            if (!worldDataStore.IsInBounds(tilePosition.x, tilePosition.y)) return false;

            if (tileSO.IsMultiTile)
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

                if (tileSO.TileType == WorldTm.ForegroundTilemap)
                {
                    ushort bgId = worldDataStore.GetTileId(tilePosition.x, tilePosition.y, WorldTm.BackgroundTilemap);
                    if (bgId != GameDataRegistry.INVALID_ID)
                    {
                        return true;
                    }
                }
            }

            return HasAdjacentSupport(tilePosition, tileSO.TileType);
        }

        private void PlaceSpecificTile(Vector2Int tilePosition, TileSO tileSO)
        {
            if (tileSO.IsMultiTile)
            {
                bool flipX = tilePosition.x + 0.5f < Player.Instance.PlayerCenter.x;
                WorldManager.Instance.WorldDataStore.SetMultiTile(tilePosition.x, tilePosition.y, tileSO, flipX);
            }
            else
            {
                ushort tileId = GameDataRegistry.Instance.GetTileIdFromTileSO(tileSO);
                switch (tileSO.TileType)
                {
                    case WorldTm.ForegroundTilemap:
                        WorldManager.Instance.WorldDataStore.SetForegroundTileId(tilePosition.x, tilePosition.y, tileId);
                        break;
                    case WorldTm.BackgroundTilemap:
                        WorldManager.Instance.WorldDataStore.SetBackgroundTileId(tilePosition.x, tilePosition.y, tileId);
                        break;
                }
            }
            
            AudioManager.Instance.PlayOneShot(tileSO.PlaceSFX, default);
        }

        public bool CanHandle(ItemSO item)
        {
            return item is TileItemSO;
        }

        public void OnSelectedStackChanged(InventoryStack stack)
        {
            _currentTileItem = !stack.IsEmpty && stack.Item is TileItemSO tileItemSO ? tileItemSO : null;
            UpdatePlacingState();
        }

        public void OnPrimaryStarted()
        {
            UpdatePlacingState();
        }

        public void OnSecondaryStarted()
        {
            UpdatePlacingState();
        }

        public void Tick()
        {
            if (_placingState != PlacingState.Placing)
            {
                return;
            }

            bool primaryHeld = GameInput.Instance != null && GameInput.Instance.PrimaryActionHeldDown;
            bool secondaryHeld = GameInput.Instance != null && GameInput.Instance.SecondaryActionHeldDown;

            if (primaryHeld)
            {
                TryToPlaceTile(true);
            }
            else if (secondaryHeld)
            {
                TryToPlaceTile(false);
            }
            else
            {
                UpdatePlacingState();
            }
        }

        private void UpdatePlacingState()
        {
            if (_currentTileItem == null)
            {
                _placingState = PlacingState.Idle;
                return;
            }

            bool primaryHeld = GameInput.Instance != null && GameInput.Instance.PrimaryActionHeldDown;
            bool secondaryHeld = GameInput.Instance != null && GameInput.Instance.SecondaryActionHeldDown;

            _placingState = (primaryHeld || secondaryHeld) ? PlacingState.Placing : PlacingState.Idle;
        }

        private void TryToPlaceTile(bool isPrimary)
        {
            if (!CanPlaceTile(isPrimary, out Vector2Int tilePosition, out TileSO tileSO))
            {
                return;
            }

            PlaceSpecificTile(tilePosition, tileSO);
            
            InventoryManager.Instance.SubtractOneFromHotbarSelectedSlot();
        }

        private bool CanPlaceTile(bool isPrimary, out Vector2Int tilePosition, out TileSO tileSO)
        {
            tilePosition = GameManager.MouseTilePosition;
            tileSO = null;
            
            if (_currentTileItem == null)
            {
                return false;
            }

            tileSO = isPrimary ? _currentTileItem.PrimaryTile : _currentTileItem.SecondaryTile;

            if (tileSO == null)
            {
                return false;
            }

            if (!isPrimary && _currentTileItem.PrimaryTile != null && _currentTileItem.PrimaryTile.IsMultiTile)
            {
                return false;
            }

            return CanPlaceSpecificTile(tilePosition, tileSO);
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

    }
}
