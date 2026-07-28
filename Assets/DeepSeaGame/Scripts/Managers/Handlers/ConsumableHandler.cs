using DeepSeaGame;
using UnityEngine;

namespace DeepSeaGame
{
    public class ConsumableHandler : MonoBehaviour, IItemUseHandler
    {
        private ConsumableItemSO _currentConsumable;
    
        public bool CanHandle(ItemSO item)
        {
            return item is ConsumableItemSO;
        }

        public void OnSelectedStackChanged(InventoryStack stack)
        {
            _currentConsumable = !stack.IsEmpty ? stack.Item as ConsumableItemSO : null;
        }

        public void OnPrimaryStarted()
        {
            if (_currentConsumable != null)
            {
                Player.Instance.Character.DamageReceiver.ReceiveHP(Player.Instance.Character, _currentConsumable.HpRestoreAmount, false);
                InventoryManager.Instance.SubtractOneFromHotbarSelectedSlot();
            }
        }

        public void OnSecondaryStarted()
        {
            
        }

        public void Tick()
        {
            
        }
    }
}
