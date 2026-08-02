using System.Text;
using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Consumable Data", menuName = "Data/Items/ComsumableItemData")]
    public class ConsumableItemSO : ItemSO
    {
        [field: Header("Consumable Parameters")]
        [field: SerializeField] public int HpRestoreAmount { get; private set; } = 4;

        public override string GetDescription()
        {
            StringBuilder description = new();
            description.Append($"+{HpRestoreAmount} HP when eaten<br>");
            description.Append($"{GetDescriptionBreak()}");

            return description.ToString();
        }
    }
}
