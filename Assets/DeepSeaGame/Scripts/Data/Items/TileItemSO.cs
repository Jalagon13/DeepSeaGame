using UnityEngine;
using UnityEngine.Serialization;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Tile Item Data", menuName = "Data/Items/TileItemData")]
    public class TileItemSO : ItemSO
    {
        [field: FormerlySerializedAs("PlaceableTile")]
        [field: SerializeField] public TileSO PrimaryTile { get; private set; }

        [field: SerializeField] public TileSO SecondaryTile { get; private set; }
    }
}
