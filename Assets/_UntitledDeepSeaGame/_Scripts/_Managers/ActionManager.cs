using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    public class ActionManager : MonoBehaviour
    {
        public static ActionManager Instance { get; private set; }
        
        private void Awake() 
        {
            Instance = this;    
        }
        
        private void Start() 
        {
            GameInput.Instance.OnPrimaryActionStarted += ExecutionItemAction;
        }
        
        private void OnDestroy() 
        {
            GameInput.Instance.OnPrimaryActionStarted -= ExecutionItemAction;
        }

        private void ExecutionItemAction(object sender, InputAction.CallbackContext e)
        {
            if(!e.started || !InventoryManager.Instance.SelectedHotbarStack.HasItem) return;
            
            ItemSO heldItem = InventoryManager.Instance.SelectedHotbarStack.Item;
            
            // if(heldItem is SpongeItemSO sponge)
            // {
            //     sponge.TryDrainAttempt();
            // }

        }
    }
}
