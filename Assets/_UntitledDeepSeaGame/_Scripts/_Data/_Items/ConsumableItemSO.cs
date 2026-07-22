using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Consumable Data", menuName = "Data/Items/ComsumableItemData")]
    public class ConsumableItemSO : ItemSO
    {
        [field: Header("Consumable Parameters")]
        [field: SerializeField] public int HungerRestore { get; private set; } = 4;
    }
}
