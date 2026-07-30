using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeepSeaGame
{
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;

        private InventoryUI _inventoryUI;
        private bool _hovered;

        public int SlotIndex { get; private set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            _inventoryUI.HandleSlotClick(SlotIndex, eventData.button);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SlotClickedSFX, default);
        }

        public void Initialize(InventoryUI inventoryUI, int slotIndex)
        {
            SlotIndex = slotIndex;
            _inventoryUI = inventoryUI;
            name = $"Inventory Slot {slotIndex + 1}";
        }

        public void Refresh(InventoryStack stack)
        {
            bool showItem = stack != null && !stack.IsEmpty && stack.Item != null;
            _iconImage.enabled = showItem && stack.Item.InventoryIcon != null;
            _iconImage.sprite = showItem ? stack.Item.InventoryIcon : null;
            _countText.text = showItem && stack.Amount > 1 ? stack.Amount.ToString() : string.Empty;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            InventoryStack invStack = InventoryManager.Instance.Slots[SlotIndex];
        
            if (invStack != null && invStack.HasItem && !InventoryManager.Instance.CursorStack.HasItem)
            {
                _hovered = true;

                Tooltip.ShowNew();

                switch (invStack.Item)
                {
                    // Add future cases here
                    
                    default:
                        int quantity = invStack.Amount;
                        string quantityString = quantity > 1 ? $"[{quantity}]" : string.Empty;
                        string itemText = $"{invStack.Item.InGameName} {quantityString}<br>{invStack.Item.GetDescription()}";

                        Tooltip.JustText(itemText, Color.white, fontSize: 12f);
                        break;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Tooltip.HideUI();
            _hovered = false;
        }
    }
}
