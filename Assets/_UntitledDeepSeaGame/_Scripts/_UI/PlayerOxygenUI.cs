using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UntitledDeepSeaGame
{
    public class PlayerOxygenUI : MonoBehaviour
    {
        [SerializeField] private Image _oxygenBarFg;
        [SerializeField] private TextMeshProUGUI _amountText;

        private void Awake()
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }

        private void OnDestroy()
        {
            Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
            if (Player.Instance != null)
            {
                Player.Instance.PlayerOxygenController.CurrentOxygen.OnValueChanged -= OnOxygenChanged;
            }
        }

        private void Player_OnAnyPlayerSpawned(object sender, Player.PlayerIdEventArgs e)
        {
            if (Player.Instance != null)
            {
                Player.Instance.PlayerOxygenController.CurrentOxygen.OnValueChanged += OnOxygenChanged;
                
                PlayerCharacterSO playerSO = Player.Instance.Character.CharacterData as PlayerCharacterSO;
                if (playerSO != null)
                {
                    UpdateView(Player.Instance.PlayerOxygenController.CurrentOxygen.Value, playerSO.BaseOxygenDuration);
                }
            }
        }

        private void OnOxygenChanged(float previousValue, float newValue)
        {
            PlayerCharacterSO playerSO = Player.Instance.Character.CharacterData as PlayerCharacterSO;
            if (playerSO != null)
            {
                UpdateView(newValue, playerSO.BaseOxygenDuration);
            }
        }

        private void UpdateView(float currentAmount, float maxAmount)
        {
            float fill = maxAmount > 0 ? currentAmount / maxAmount : 0f;
            _oxygenBarFg.fillAmount = fill;
            _amountText.text = $"{Mathf.CeilToInt(currentAmount)}/{maxAmount}";
        }
    }
}
