using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HabitatCoreInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<RecipeSO> _availableRecipes;

        public bool CanInteract => true;

        public void Interact()
        {
            Debug.Log($"Habitat core interacted: {name}");
        }
    }
}
