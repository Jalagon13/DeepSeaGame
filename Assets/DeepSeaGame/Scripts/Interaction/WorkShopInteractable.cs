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
            InventoryManager.Instance.OpenInventory();
            CraftingMenuUI.Instance.PopulateRecipes(_availableRecipes);
        }

        public void OnFlipX()
        {
            
        }
    }
}
