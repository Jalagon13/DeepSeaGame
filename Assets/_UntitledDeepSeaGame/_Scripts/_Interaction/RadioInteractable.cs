using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    public class RadioInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<RecipeSO> _availableRecipes;

        public bool CanInteract => true;

        public void Interact()
        {
            Debug.Log($"Interacting with {gameObject.name}");
            GameManager.Instance.OnPrototypeEnd();
        }

        public void OnFlipX()
        {

        }
    }
}
