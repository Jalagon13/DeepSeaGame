using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSeaGame
{
    public class RecipePanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _recipeBackground;
        
        [Header("Craftable")]
        [SerializeField] private Color _craftableBgColor;
        [SerializeField] private Color _craftableTextColor;

        [Header("UnCraftable")]
        [SerializeField] private Color _uncCraftableBgColor;
        [SerializeField] private Color _unCraftableTextColor;

        private RecipeSO _recipe;
        private CraftingMenuUI _craftingMenuUI;

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= CheckCraftability;
            }
        }

        public void Setup(RecipeSO recipe, CraftingMenuUI craftingMenuUI)
        {
            _nameText.text = $"{recipe.OutputItem.InGameName} [x{recipe.OutputAmount}]";
            _iconImage.sprite = recipe.OutputItem.InventoryIcon;
            _recipe = recipe;
            _craftingMenuUI = craftingMenuUI;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += CheckCraftability;
                CheckCraftability();
            }
        }

        private void CheckCraftability()
        {
            if (_recipe == null || InventoryManager.Instance == null) return;

            bool canCraft = true;
            foreach (ItemRequirement req in _recipe.Requirements)
            {
                if (!InventoryManager.Instance.HasItemAmount(req.Item, req.Amount))
                {
                    canCraft = false;
                    break;
                }
            }

            if (canCraft)
            {
                SetCraftableVisuals();
            }
            else
            {
                SetUncraftableVisuals();
            }
        }

        private void SetCraftableVisuals()
        {
            _recipeBackground.color = _craftableBgColor;
            _nameText.color = _craftableTextColor;
        }

        private void SetUncraftableVisuals()
        {
            _recipeBackground.color = _uncCraftableBgColor;
            _nameText.color = _unCraftableTextColor;
        }

        public void OnRecipePanelClicked()
        {
            _craftingMenuUI.SelectedRecipe = _recipe;
        }
    }
}