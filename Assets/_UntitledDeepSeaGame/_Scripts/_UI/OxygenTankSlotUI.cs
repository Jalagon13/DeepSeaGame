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
            InventoryStack cursorStack = InventoryManager.Instance.CursorStack;
            
            if(OxygenTankEquipped())
            {
                if(cursorStack.HasItem)
                {
                    if(cursorStack.Item is OxygenTankItemSO cursorOxygenTank)
                    {
                        // Swap the equipped tank with the one on the cursor
                        OxygenTankItemSO equippedTank = Player.Instance.PlayerOxygenController.EquippedOxygenTank;
                        Player.Instance.PlayerOxygenController.UnequipOxygenTank();
                        Player.Instance.PlayerOxygenController.EquipOxygenTank(cursorOxygenTank);
                        InventoryManager.Instance.CursorStack.Set(equippedTank, 1);
                    }
                }
                else
                {
                    OxygenTankItemSO equippedTank = Player.Instance.PlayerOxygenController.EquippedOxygenTank;
                    Player.Instance.PlayerOxygenController.UnequipOxygenTank();
                    InventoryManager.Instance.CursorStack.Set(equippedTank, 1);
                }
            }
            else if(cursorStack.HasItem && cursorStack.Item is OxygenTankItemSO cursorOxygenTank)
            {
                Player.Instance.PlayerOxygenController.EquipOxygenTank(cursorOxygenTank);
                InventoryManager.Instance.CursorStack.Clear();
            }
            
            RefreshUI();
            InventoryManager.Instance.RefreshAfterInventoryChange();
        }
        
        private bool OxygenTankEquipped()
        {
            return Player.Instance.PlayerOxygenController.EquippedOxygenTank != null;
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
    }
}
