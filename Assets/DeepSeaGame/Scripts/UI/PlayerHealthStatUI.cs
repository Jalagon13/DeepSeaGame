using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSeaGame
{
    public class PlayerHealthStatUI : MonoBehaviour
    {
        [SerializeField] private Image _healthBarFg;
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
                Player.Instance.Character.NetHealthState.OnHitPointsChanged -= Player_OnPlayerHealthUpdated;
            }
        }

        private void Player_OnAnyPlayerSpawned(object sender, Player.PlayerIdEventArgs e)
        {
            if (Player.Instance != null)
            {
                Player.Instance.Character.NetHealthState.OnHitPointsChanged += Player_OnPlayerHealthUpdated;
                UpdateView(Player.Instance.Character.CharacterData.BaseMaxHealth, Player.Instance.Character.CharacterData.BaseMaxHealth);
            }
        }

        private void Player_OnPlayerHealthUpdated(object sender, NetworkHealthState.PointsChangedEventArgs e)
        {
            UpdateView(e.CurrentPoints, e.MaxPoints);
        }

        private void UpdateView(int currentAmount, int maxAmount)
        {
            float fill = maxAmount > 0 ? (float)currentAmount / maxAmount : 0f;
            // Debug.Log($"currentAmount: {currentAmount}, maxAmount: {maxAmount}, fill amount {fill}");
            _healthBarFg.fillAmount = fill;
            _amountText.text = $"HP: {currentAmount}/{maxAmount}";
        }
    }
}
