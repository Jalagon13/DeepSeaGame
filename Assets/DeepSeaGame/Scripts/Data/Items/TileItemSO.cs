using System.Text;
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

        public override string GetDescription()
        {
            StringBuilder description = new();
            description.Append($"Can be placed.<br>");
            description.Append($"{GetDescriptionBreak()}");

            return description.ToString();
        }
    }
}
