using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UntitledDeepSeaGame
{
    public class GameDataRegistry : MonoBehaviour
    {
        public static GameDataRegistry Instance { get; private set; }
        public const ushort INVALID_ID = ushort.MaxValue;
        

        [SerializeField]
        private List<ItemSO> _itemData;

        [Space(15)]
        [SerializeField]
        private List<TileSO> _tileData;

        private void Awake()
        {
            Instance = this;
        }

        #region Item Data Functions

        public ushort GetItemIdFromItemSO(ItemSO itemData)
        {
            if (itemData == null)
            {
                return INVALID_ID;
            }

            for (int i = 0; i < _itemData.Count; i++)
            {
                if (_itemData[i].ItemName == itemData.ItemName)
                {
                    return (ushort)i;
                }
            }

            Debug.LogError($"ItemDataSO '{itemData}' not found!");
            return ushort.MaxValue;
        }

        public ItemSO GetItemSOFromItemId(ushort itemId)
        {
            if (itemId >= _itemData.Count || itemId < 0)
            {
                // Debug.LogError($"Invalid Item ID: {itemId}");
                return null;
            }

            return _itemData[itemId];
        }

        #endregion

        #region Tile Data Functions

        public TileSO GetTileSOFromTileId(ushort tileId)
        {
            if (tileId >= _tileData.Count || tileId < 0)
            {
                // Debug.LogError($"Invalid Tile ID: {tileId}");
                return null;
            }

            return _tileData[tileId];
        }

        public ushort GetTileIdFromTileSO(TileSO tileSO)
        {
            if (tileSO == null)
            {
                Debug.LogError($"TileDataSO is null. Use this log to deduce where this came from");
            }

            for (int i = 0; i < _tileData.Count; i++)
            {
                if (_tileData[i].StringID == tileSO.StringID)
                {
                    return (ushort)i;
                }
            }

            Debug.LogError($"TileDataSO '{tileSO}' not found!");
            return ushort.MaxValue;
        }

        public ushort GetTileIdFromTileBase(TileBase tileBase)
        {
            return GetTileIdFromTileSO(GetTileSOFromTileBase(tileBase));
        }

        public TileSO GetTileSOFromTileBase(TileBase tileBase)
        {
            foreach (TileSO tileSO in _tileData)
            {
                if (tileSO == tileBase)
                {
                    return tileSO;
                }
            }

            Debug.LogError($"Cannot find {tileBase} in TileObjectSOList, returning default");
            return default;
        }

        #endregion


    }
}
