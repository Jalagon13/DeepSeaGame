using System.Text;
using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Oxygen Tank Item Data", menuName = "Data/Items/OxygenTankItemData")]
    public class OxygenTankItemSO : ItemSO
    {
        [field: SerializeField] public int AdditionalOxygen { get; private set; } = 0;
        [field: SerializeField] public float OxygenRecoveryDuration { get; private set; } = 3;

        public override string GetDescription()
        {
            StringBuilder description = new();
            description.Append($"Can be placed on Tank Slot<br>");
            description.Append($"{GetDescriptionBreak()}");

            return description.ToString();
        }
    }
}
