using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Sponge Data", menuName = "Data/SpongeData")]
    public class SpongeItemSO : ItemSO
    {
        [field: SerializeField]
        public int MaxTileDetection { get; private set; } = 40;
        
        public void TryDrainAttempt()
        {
            Debug.Log($"Attempting drain attempt");
            
            
        }
    }
}
