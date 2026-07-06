using UnityEngine;
using System.Collections.Generic;
using System;

namespace UntitledDeepSeaGame
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
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _activeRecipes = _defaultRecipes;
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

        public void ShowCraftingMenu(List<RecipeSO> recipes = null)
        {
            Debug.Log("ShowCraftingMenu called");
            if (recipes != null)
            {
                Debug.Log($"Setting active recipes to provided list with {recipes.Count} recipes.");
                _activeRecipes = recipes;
            }
            else
            {
                Debug.Log("No recipes provided, using default recipes.");
                _activeRecipes = _defaultRecipes;
            }

            gameObject.SetActive(true);
            ClearRecipeListPanelUI();
            PopulateRecipeListPanelUI();

            CraftingMenuUIOpen = true;
        }

        public void HideCraftingMenu()
        {
            _activeRecipes = _defaultRecipes;
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
