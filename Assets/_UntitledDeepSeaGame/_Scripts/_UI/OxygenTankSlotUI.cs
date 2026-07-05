using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UntitledDeepSeaGame
{
    public class OxygenTankSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;


        public void OnPointerClick(PointerEventData eventData)
        {
            if (Player.Instance.PlayerOxygenController == null)
            {
                Debug.LogWarning("PlayerOxygenController not initialized.");
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                HandleLeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            // Get the cursor stack (item being dragged)
            InventoryStack cursorStack = InventoryManager.Instance.CursorStack;

            // If cursor has an oxygen tank, equip it
            if (!cursorStack.IsEmpty && cursorStack.Item is OxygenTankItemSO oxygenTank)
            {
                EquipOxygenTank(oxygenTank, cursorStack);
                return;
            }

            // If cursor is empty and a tank is equipped, remove the equipped tank
            if (cursorStack.IsEmpty && Player.Instance.PlayerOxygenController.EquippedOxygenTank != null)
            {
                HandleRightClick();
            }
        }

        private void HandleRightClick()
        {
            // If no tank is equipped, do nothing
            if (Player.Instance.PlayerOxygenController.EquippedOxygenTank == null)
            {
                return;
            }

            // Unequip the oxygen tank
            OxygenTankItemSO equippedTank = Player.Instance.PlayerOxygenController.EquippedOxygenTank;
            Player.Instance.PlayerOxygenController.UnequipOxygenTank();

            // Add the unequipped tank back to inventory
            InventoryManager.Instance.AddItem(equippedTank, 1);

            // Refresh the UI
            RefreshUI();
        }

        private void EquipOxygenTank(OxygenTankItemSO oxygenTank, InventoryStack cursorStack)
        {
            // If there's already an equipped tank, unequip it first and add it to inventory
            if (Player.Instance.PlayerOxygenController.EquippedOxygenTank != null)
            {
                OxygenTankItemSO previousTank = Player.Instance.PlayerOxygenController.EquippedOxygenTank;
                Player.Instance.PlayerOxygenController.UnequipOxygenTank();
                InventoryManager.Instance.AddItem(previousTank, 1);
            }

            // Equip the new oxygen tank
            Player.Instance.PlayerOxygenController.EquipOxygenTank(oxygenTank);

            // Remove the item from the cursor/inventory
            InventoryManager.Instance.RemoveItem(oxygenTank, 1);

            // Refresh the UI
            RefreshUI();
        }

        public void RefreshUI()
        {
            OxygenTankItemSO equippedTank = Player.Instance.PlayerOxygenController.EquippedOxygenTank;

            if (equippedTank != null && equippedTank.InventoryIcon != null)
            {
                _iconImage.enabled = true;
                _iconImage.sprite = equippedTank.InventoryIcon;
                if (_countText != null)
                {
                    _countText.text = "1";
                }
            }
            else
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;
                if (_countText != null)
                {
                    _countText.text = string.Empty;
                }
            }
        }

        // Call this from InventoryUI or another manager to refresh when inventory changes
        public void OnInventoryChanged()
        {
            RefreshUI();
        }
    }
}
