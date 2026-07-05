using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Tile Item Data", menuName = "Data/Items/TileItemData")]
    public class TileItemSO : ItemSO
    {
        [field: SerializeField] public TileSO PlaceableTile { get; private set; }
    }
}
