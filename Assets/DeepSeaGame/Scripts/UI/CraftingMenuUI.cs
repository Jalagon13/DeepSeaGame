using UnityEngine;
using System.Collections.Generic;
using System;

namespace DeepSeaGame
{
    public class CraftingMenuUI : MonoBehaviour
    {
        public static CraftingMenuUI Instance { get; private set; }

        [SerializeField] private GameObject _recipeListPanelUI;
        [SerializeField] private RecipePanelUI _recipePanelUIPrefab;

        [SerializeField] private CraftItemPanelUI _craftItemPanelUI;
        public CraftItemPanelUI CraftItemPanelUI => _craftItemPanelUI;

        [SerializeField] private List<RecipeSO> _defaultRecipes;

        private RecipeSO _selectedRecipe;
        private List<RecipeSO> _activeRecipes;

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

        private void Awake()
        {
            Instance = this;
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
            _activeRecipes = _defaultRecipes;
        }

        public void PopulateRecipes(List<RecipeSO> recipes)
        {
            if (recipes != null)
            {
                _activeRecipes = recipes;
            }
            else
            {
                _activeRecipes = _defaultRecipes;
            }
            
            ClearRecipeListPanelUI();
            PopulateRecipeListPanelUI();
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
            if (_activeRecipes == null)
            {
                return;
            }

            for (int i = 0; i < _activeRecipes.Count; i++)
            {
                RecipeSO recipe = _activeRecipes[i];
                RecipePanelUI recipePanelUI = Instantiate(_recipePanelUIPrefab.gameObject, _recipeListPanelUI.transform).GetComponent<RecipePanelUI>();
                recipePanelUI.Setup(recipe, this);
            }
        }
    }
}
