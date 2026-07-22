using DeepSeaGame;
using UnityEngine;

namespace DeepSeaGame
{
    public class ConsumableHandler : MonoBehaviour, IItemUseHandler
    {
        public bool CanHandle(ItemSO item)
        {
            return item is ConsumableItemSO;
        }

        public void OnPrimaryStarted()
        {
            
        }

        public void OnSecondaryStarted()
        {
            
        }

        public void OnSelectedStackChanged(InventoryStack stack)
        {
            
        }

        public void Tick()
        {
            
        }
    }
}
