using UnityEngine;

namespace DeepSeaGame
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact();
        void OnFlipX();
    }
}
