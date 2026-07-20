using UnityEngine;

namespace UntitledDeepSeaGame
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact();
        void OnFlipX();
    }
}
