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

        private RecipeSO _recipe;
        private CraftingMenuUI _craftingMenuUI;

        public void Setup(RecipeSO recipe, CraftingMenuUI craftingMenuUI)
        {
            _nameText.text = $"{recipe.OutputItem.ItemName} [x{recipe.OutputAmount}]";
            _iconImage.sprite = recipe.OutputItem.InventoryIcon;
            _recipe = recipe;
            _craftingMenuUI = craftingMenuUI;
        }

        public void OnRecipePanelClicked()
        {
            _craftingMenuUI.SelectedRecipe = _recipe;
        }
    }
}