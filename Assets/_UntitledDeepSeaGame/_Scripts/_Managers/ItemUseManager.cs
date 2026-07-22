using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepSeaGame
{
    public interface IItemUseHandler
    {
        bool CanHandle(ItemSO item);
        void OnSelectedStackChanged(InventoryStack stack);
        void OnPrimaryStarted();
        void OnSecondaryStarted();
        void Tick();
    }

    public class ItemUseManager : MonoBehaviour
    {
        public static ItemUseManager Instance { get; private set; }

        [SerializeField] private MonoBehaviour[] _handlerBehaviours;

        private readonly List<IItemUseHandler> _handlers = new();
        private IItemUseHandler _activeHandler;

        private void Awake()
        {
            Instance = this;

            foreach (MonoBehaviour behaviour in _handlerBehaviours)
            {
                if (behaviour is IItemUseHandler handler)
                {
                    _handlers.Add(handler);
                }
            }
        }

        private void Start()
        {
            GameInput.Instance.OnPrimaryActionStarted += OnPrimaryActionStarted;
            GameInput.Instance.OnSecondaryActionStarted += OnSecondaryActionStarted;
            InventoryManager.Instance.OnSelectedHotbarSlotChanged += OnSelectedHotbarSlotChanged;

            OnSelectedHotbarSlotChanged(InventoryManager.Instance.SelectedHotbarSlotIndex, InventoryManager.Instance.SelectedHotbarStack);
        }

        private void OnDestroy()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnPrimaryActionStarted -= OnPrimaryActionStarted;
                GameInput.Instance.OnSecondaryActionStarted -= OnSecondaryActionStarted;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnSelectedHotbarSlotChanged -= OnSelectedHotbarSlotChanged;
            }
        }

        private void Update()
        {
            _activeHandler?.Tick();
        }

        private void OnSelectedHotbarSlotChanged(int slotIndex, InventoryStack stack)
        {
            _activeHandler = null;

            foreach (IItemUseHandler handler in _handlers)
            {
                handler.OnSelectedStackChanged(stack);
            }

            if (stack.IsEmpty)
            {
                return;
            }

            foreach (IItemUseHandler handler in _handlers)
            {
                if (handler.CanHandle(stack.Item))
                {
                    _activeHandler = handler;
                    return;
                }
            }
        }

        private void OnPrimaryActionStarted(object sender, InputAction.CallbackContext e)
        {
            if (!e.started)
            {
                return;
            }

            _activeHandler?.OnPrimaryStarted();
        }

        private void OnSecondaryActionStarted(object sender, InputAction.CallbackContext e)
        {
            if (!e.started)
            {
                return;
            }

            _activeHandler?.OnSecondaryStarted();
        }

        
    }
}
