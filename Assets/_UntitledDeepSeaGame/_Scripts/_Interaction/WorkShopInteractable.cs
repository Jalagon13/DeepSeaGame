using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    public class WorkShopInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<RecipeSO> _availableRecipes;

        public bool CanInteract => true;

        public void Interact()
        {
            Debug.Log($"Interacting with {gameObject.name}");
            InventoryManager.Instance?.OpenInventory(_availableRecipes);
        }

        public void OnFlipX()
        {
            
        }
    }
}
