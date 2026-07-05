using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Oxygen Tank Item Data", menuName = "Data/Items/OxygenTankItemData")]
    public class OxygenTankItemSO : ItemSO
    {
        [field: SerializeField] public int AdditionalOxygen { get; private set; } = 0;
        [field: SerializeField] public float OxygenRecoveryDuration { get; private set; } = 3;
    }
}
