using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSeaGame
{
    public class CraftItemPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject _contentPanel;
        [SerializeField] private Image _outputItemImage;
        [SerializeField] private TextMeshProUGUI _outputItemNameText;
        [SerializeField] private GameObject _ingredientListSectionUI;
        [SerializeField] private IngredientPanelUI _ingredientPanelUIPrefab;
        [SerializeField] private Image _craftButtonCantCraftOverlay;

        private List<IngredientPanelUI> _ingredientList;

        private RecipeSO _currentRecipe;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateCraftButtonOverlay;
        }


        private void OnDestroy()
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateCraftButtonOverlay;
        }

        public void UpdatePanel(RecipeSO recipe)
        {
            _currentRecipe = recipe;
            _craftButtonCantCraftOverlay.enabled = false;

            PopulateCraftItemPanelUI(recipe);
            UpdateCraftButtonOverlay();
            Show();
        }

        private void PopulateCraftItemPanelUI(RecipeSO recipe)
        {
            _outputItemImage.sprite = recipe.OutputItem.InventoryIcon;
            _outputItemNameText.text = recipe.OutputItem.InGameName;

            ClearIngredientListSectionUI();
            PopulateIngredientListSectionUI(recipe);
        }

        private void ClearIngredientListSectionUI()
        {
            foreach (Transform child in _ingredientListSectionUI.transform)
            {
                Destroy(child.gameObject);
            }

            if (_ingredientList == null)
            {
                _ingredientList = new();
            }
            else
            {
                _ingredientList.Clear();
            }
        }

        private void PopulateIngredientListSectionUI(RecipeSO recipe)
        {
            for (int i = 0; i < recipe.Requirements.Count; i++)
            {
                ItemRequirement itemRequirement = recipe.Requirements[i];
                IngredientPanelUI ingredientPanelUI = Instantiate(_ingredientPanelUIPrefab.gameObject, _ingredientListSectionUI.transform).GetComponent<IngredientPanelUI>();
                ingredientPanelUI.Setup(itemRequirement);

                _ingredientList.Add(ingredientPanelUI);
            }
        }

        public void OnCraftButtonPressed()
        {
            foreach (IngredientPanelUI ingredientPanelUI in _ingredientList)
            {
                if (!ingredientPanelUI.HasIngredient)
                {
                    return;
                }
            }
            HandleCraftingComplete();
            UpdateCraftButtonOverlay();
        }

        private void HandleCraftingComplete()
        {
            foreach (var item in _currentRecipe.Requirements)
            {
                InventoryManager.Instance.RemoveItem(item.Item, item.Amount);
            }

            InventoryManager.Instance.AddItem(_currentRecipe.OutputItem, _currentRecipe.OutputAmount);
        }


        private void UpdateCraftButtonOverlay()
        {
            if (_ingredientList == null || _ingredientList.Count == 0)
            {
                _craftButtonCantCraftOverlay.enabled = true;
                return;
            }

            foreach (IngredientPanelUI ingredientPanelUI in _ingredientList)
            {
                if (!ingredientPanelUI.HasIngredient)
                {
                    _craftButtonCantCraftOverlay.enabled = true;
                    return;
                }
            }

            _craftButtonCantCraftOverlay.enabled = false;
        }

        public void Show()
        {
            _contentPanel.SetActive(true);
        }

        private void Hide()
        {
            _contentPanel.SetActive(false);
        }
    }
}
