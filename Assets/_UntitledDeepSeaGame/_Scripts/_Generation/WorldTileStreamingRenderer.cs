using UnityEngine;
using UnityEngine.Tilemaps;

namespace UntitledDeepSeaGame
{
    public class WorldTileStreamingRenderer : MonoBehaviour
    {
        private WorldDataStore _worldDataStore;
        private Tilemap _foregroundTilemap;
        private Tilemap _backgroundTilemap;
        private RectInt _renderedBounds;
        private bool _isInitialized;
        private bool _hasRenderedBounds;

        private void OnEnable()
        {
            PlayerCamera.OnVisibleTileBoundsChanged += HandleVisibleTileBoundsChanged;
        }

        private void OnDisable()
        {
            PlayerCamera.OnVisibleTileBoundsChanged -= HandleVisibleTileBoundsChanged;

            if (_worldDataStore != null)
            {
                _worldDataStore.TileChanged -= HandleTileChanged;
            }
        }

        public void Initialize(WorldDataStore worldDataStore, Tilemap foregroundTilemap, Tilemap backgroundTilemap)
        {
            if (_worldDataStore != null)
            {
                _worldDataStore.TileChanged -= HandleTileChanged;
            }

            _worldDataStore = worldDataStore;
            _foregroundTilemap = foregroundTilemap;
            _backgroundTilemap = backgroundTilemap;
            _isInitialized = _worldDataStore != null && _foregroundTilemap != null && _backgroundTilemap != null;
            _hasRenderedBounds = false;
            _renderedBounds = default;

            if (!_isInitialized)
            {
                Debug.LogWarning("WorldTileStreamingRenderer could not initialize because required references are missing.");
                return;
            }

            _worldDataStore.TileChanged += HandleTileChanged;
            _foregroundTilemap.ClearAllTiles();
            _backgroundTilemap.ClearAllTiles();

            if (PlayerCamera.Instance != null)
            {
                HandleVisibleTileBoundsChanged(PlayerCamera.Instance.CurrentVisibleTileBounds);
            }
        }

        private void HandleVisibleTileBoundsChanged(RectInt visibleBounds)
        {
            if (!_isInitialized)
            {
                return;
            }

            RectInt clampedBounds = ClampToWorldBounds(visibleBounds);

            if (!_hasRenderedBounds)
            {
                RenderRect(clampedBounds);
                _renderedBounds = clampedBounds;
                _hasRenderedBounds = true;
                return;
            }

            if (_renderedBounds == clampedBounds)
            {
                return;
            }

            if (!_renderedBounds.Overlaps(clampedBounds))
            {
                ClearRect(_renderedBounds);
                RenderRect(clampedBounds);
                _renderedBounds = clampedBounds;
                return;
            }

            UpdateRenderedBounds(_renderedBounds, clampedBounds);
            _renderedBounds = clampedBounds;
        }

        private void HandleTileChanged(Vector2Int tilePosition, ushort previousTileId, ushort newTileId, WorldTm targetMap)
        {
            if (!_isInitialized || !_hasRenderedBounds || !_renderedBounds.Contains(tilePosition))
            {
                return;
            }

            ApplyTile(tilePosition.x, tilePosition.y, targetMap);
        }

        private RectInt ClampToWorldBounds(RectInt bounds)
        {
            if (_worldDataStore.Width == 0 || _worldDataStore.Height == 0)
            {
                return new RectInt(0, 0, 0, 0);
            }

            int minX = Mathf.Clamp(bounds.xMin, 0, _worldDataStore.Width);
            int minY = Mathf.Clamp(bounds.yMin, 0, _worldDataStore.Height);
            int maxX = Mathf.Clamp(bounds.xMax, 0, _worldDataStore.Width);
            int maxY = Mathf.Clamp(bounds.yMax, 0, _worldDataStore.Height);

            if (maxX < minX)
            {
                maxX = minX;
            }

            if (maxY < minY)
            {
                maxY = minY;
            }

            return CreateRectFromMinMax(minX, minY, maxX, maxY);
        }

        private void UpdateRenderedBounds(RectInt previousBounds, RectInt currentBounds)
        {
            ClearColumnsOutside(previousBounds, currentBounds);
            ClearRowsOutside(previousBounds, currentBounds);
            RenderColumnsOutside(previousBounds, currentBounds);
            RenderRowsOutside(previousBounds, currentBounds);
        }

        private void ClearColumnsOutside(RectInt previousBounds, RectInt currentBounds)
        {
            if (currentBounds.xMin > previousBounds.xMin)
            {
                ClearRect(CreateRectFromMinMax(previousBounds.xMin, previousBounds.yMin, currentBounds.xMin, previousBounds.yMax));
            }

            if (currentBounds.xMax < previousBounds.xMax)
            {
                ClearRect(CreateRectFromMinMax(currentBounds.xMax, previousBounds.yMin, previousBounds.xMax, previousBounds.yMax));
            }
        }

        private void ClearRowsOutside(RectInt previousBounds, RectInt currentBounds)
        {
            int overlapMinX = Mathf.Max(previousBounds.xMin, currentBounds.xMin);
            int overlapMaxX = Mathf.Min(previousBounds.xMax, currentBounds.xMax);

            if (overlapMaxX <= overlapMinX)
            {
                return;
            }

            if (currentBounds.yMin > previousBounds.yMin)
            {
                ClearRect(CreateRectFromMinMax(overlapMinX, previousBounds.yMin, overlapMaxX, currentBounds.yMin));
            }

            if (currentBounds.yMax < previousBounds.yMax)
            {
                ClearRect(CreateRectFromMinMax(overlapMinX, currentBounds.yMax, overlapMaxX, previousBounds.yMax));
            }
        }

        private void RenderColumnsOutside(RectInt previousBounds, RectInt currentBounds)
        {
            if (currentBounds.xMin < previousBounds.xMin)
            {
                RenderRect(CreateRectFromMinMax(currentBounds.xMin, currentBounds.yMin, previousBounds.xMin, currentBounds.yMax));
            }

            if (currentBounds.xMax > previousBounds.xMax)
            {
                RenderRect(CreateRectFromMinMax(previousBounds.xMax, currentBounds.yMin, currentBounds.xMax, currentBounds.yMax));
            }
        }

        private void RenderRowsOutside(RectInt previousBounds, RectInt currentBounds)
        {
            int overlapMinX = Mathf.Max(previousBounds.xMin, currentBounds.xMin);
            int overlapMaxX = Mathf.Min(previousBounds.xMax, currentBounds.xMax);

            if (overlapMaxX <= overlapMinX)
            {
                return;
            }

            if (currentBounds.yMin < previousBounds.yMin)
            {
                RenderRect(CreateRectFromMinMax(overlapMinX, currentBounds.yMin, overlapMaxX, previousBounds.yMin));
            }

            if (currentBounds.yMax > previousBounds.yMax)
            {
                RenderRect(CreateRectFromMinMax(overlapMinX, previousBounds.yMax, overlapMaxX, currentBounds.yMax));
            }
        }

        private void RenderRect(RectInt bounds)
        {
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                return;
            }

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    ApplyTile(x, y);
                }
            }
        }

        private void ClearRect(RectInt bounds)
        {
            if (!_isInitialized || bounds.width <= 0 || bounds.height <= 0)
            {
                return;
            }

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    _foregroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    _backgroundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }

        private void ApplyTile(int x, int y)
        {
            ApplyTile(x, y, WorldTm.ForegroundTilemap);
            ApplyTile(x, y, WorldTm.BackgroundTilemap);
        }

        private void ApplyTile(int x, int y, WorldTm targetMap)
        {
            ushort tileId = _worldDataStore.GetTileId(x, y, targetMap);
            TileBase tile = tileId == GameDataRegistry.INVALID_ID ? null : GameDataRegistry.Instance.GetTileSOFromTileId(tileId);

            if (targetMap == WorldTm.ForegroundTilemap)
            {
                _foregroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
            else
            {
                _backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        private RectInt CreateRectFromMinMax(int minX, int minY, int maxX, int maxY)
        {
            return new RectInt(minX, minY, Mathf.Max(0, maxX - minX), Mathf.Max(0, maxY - minY));
        }
    }
}
