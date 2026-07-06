using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }

        [SerializeField] private LayerMask _interactionMask = ~0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameInput.Instance.OnInteract += GameInput_OnInteract;
        }

        private void OnDestroy()
        {
            GameInput.Instance.OnInteract -= GameInput_OnInteract;
        }

        private void GameInput_OnInteract(object sender, InputAction.CallbackContext context)
        {
            Vector2 mouseWorldPosition = GameManager.MouseWorldPosition;
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition, _interactionMask);
            if (hits == null || hits.Length == 0)
            {
                return;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i];
                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    interactable.Interact();
                    return;
                }
            }
        }
    }
}
