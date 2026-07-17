using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Tool Data", menuName = "Data/Items/ToolItemData")]
    public class ToolItemSO : ItemSO
    {
        [field: Header("Tool Parameters")]
        [field: SerializeField] public ToolType HarvestType { get; private set; }
        [field: SerializeField] public HeldObject HeldObject { get; private set; } 
        [field: SerializeField] public int Damage { get; private set; } = 4;
        [field: SerializeField] public int Knockback { get; private set; } = 6;
        [field: SerializeField] public float AttackDuration { get; private set; } = 0.35f;
        [field: SerializeField] public float ThrustDistance { get; private set; } = 3f;
        [field: SerializeField] public float MiningPower { get; private set; } = 1f;
        
    }
}
