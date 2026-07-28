using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Recipe Data", menuName = "Data/RecipeData")]
    public class RecipeSO : ScriptableObject
    {
        [field: SerializeField] public ItemSO OutputItem { get; private set; }
        [field: SerializeField] public int OutputAmount { get; private set; } = 1;
        [field: SerializeField] public List<ItemRequirement> Requirements { get; private set; } = new();
    }

    [Serializable]
    public struct ItemRequirement
    {
        [field: SerializeField] public ItemSO Item { get; private set; }
        [field: SerializeField] public int Amount { get; private set; }
    }
}
