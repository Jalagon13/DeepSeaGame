using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Unity.Netcode;
using UnityEngine;

namespace DeepSeaGame
{
    public class OxygenWarningUI : NetworkBehaviour
    {
        [SerializeField] private float _displayTimer = 3f;
        [SerializeField] private RectTransform _warningPanel;

        [Header("Scale Pulse Settings")]
        [SerializeField] private int _pulseCount = 2;
        [SerializeField] private float _pulseScale = 1.5f;
        [SerializeField] private float _pulseDuration = 0.4f;

        private void Awake()
        {
            DisableWarningPanel();
            Player.OnAnyPlayerSpawned += RegisterOxygenWarning;
        }

        public override void OnDestroy()
        {
            Player.OnAnyPlayerSpawned -= RegisterOxygenWarning;

            if (Player.Instance != null)
            {
                Player.Instance.PlayerOxygenController.OnOxygenWarning -= OnOxygenWarning;
            }
        }

        private void RegisterOxygenWarning(object sender, Player.PlayerIdEventArgs e)
        {
            if (NetworkManager.LocalClientId != e.PlayerId) return;

            Player.Instance.PlayerOxygenController.OnOxygenWarning += OnOxygenWarning;
        }

        private void OnOxygenWarning()
        {
            Debug.Log("Oxygen Warning");
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.OxygenWarningSFX, default);

            EnableWarningPanel();
            StopAllCoroutines();
            StartCoroutine(DisableWarningPanelAfterDelay());
        }

        private IEnumerator DisableWarningPanelAfterDelay()
        {
            yield return new WaitForSeconds(_displayTimer);
            DisableWarningPanel();
        }

        private void EnableWarningPanel()
        {
            _warningPanel.gameObject.SetActive(true);
            _warningPanel.localScale = Vector3.one;

            _warningPanel.DOKill();

            Sequence pulseSequence = DOTween.Sequence();
            
            for (int i = 0; i < _pulseCount; i++)
            {
                pulseSequence.Append(_warningPanel.DOScale(_pulseScale, _pulseDuration * 0.5f).SetEase(Ease.InOutSine));
                pulseSequence.Append(_warningPanel.DOScale(1f, _pulseDuration * 0.5f).SetEase(Ease.InOutSine));
            }
            
            pulseSequence.Play();
        }

        private void DisableWarningPanel()
        {
            _warningPanel.DOKill();
            _warningPanel.gameObject.SetActive(false);
        }
    }
}