using UnityEngine;
using System.Collections.Generic;
using System;

namespace UntitledDeepSeaGame
{
    public class CraftingMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _recipeListPanelUI;
        [SerializeField] private RecipePanelUI _recipePanelUIPrefab;

        [SerializeField] private CraftItemPanelUI _craftItemPanelUI;
        public CraftItemPanelUI CraftItemPanelUI => _craftItemPanelUI;

        [SerializeField] private List<RecipeSO> _defaultRecipes;

        private RecipeSO _selectedRecipe;

        public bool CraftingMenuUIOpen { get; private set; }

        public RecipeSO SelectedRecipe
        {
            get { return _selectedRecipe; }
            set
            {
                _selectedRecipe = value;
                _craftItemPanelUI.UpdatePanel(_selectedRecipe);
            }
        }

        private void Start()
        {
            HideCraftingMenu();
        }

        private void OnEnable()
        {
            ClearRecipeListPanelUI();
            PopulateRecipeListPanelUI();
            CraftingMenuUIOpen = true;
        }

        private void OnDisable()
        {
            ClearRecipeListPanelUI();
            CraftingMenuUIOpen = false;
        }

        private void ShowCraftingMenu()
        {
            gameObject.SetActive(true);
            ClearRecipeListPanelUI();
            PopulateRecipeListPanelUI();

            CraftingMenuUIOpen = true;
        }

        private void HideCraftingMenu()
        {
            gameObject.SetActive(false);

            CraftingMenuUIOpen = false;
        }

        private void ClearRecipeListPanelUI()
        {
            foreach (Transform child in _recipeListPanelUI.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void PopulateRecipeListPanelUI()
        {
            for (int i = 0; i < _defaultRecipes.Count; i++)
            {
                RecipeSO recipe = _defaultRecipes[i];
                RecipePanelUI recipePanelUI = Instantiate(_recipePanelUIPrefab.gameObject, _recipeListPanelUI.transform).GetComponent<RecipePanelUI>();
                recipePanelUI.Setup(recipe, this);
            }
        }
    }
}
