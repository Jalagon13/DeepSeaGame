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
        [Header("First Oxygen Warning")]
        [SerializeField] private float _firstDisplayTimer = 3f;
        [SerializeField] private RectTransform _firstWarningPanel;
        
        
        [Header("Second Oxygen Warning")]
        [SerializeField] private float _secondDisplayTimer = 3f;
        [SerializeField] private RectTransform _secondWarningPanel;
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
                Player.Instance.PlayerOxygenController.OnFirstOxygenWarning -= OnFirstOxygenWarning;
                Player.Instance.PlayerOxygenController.OnSecondOxygenWarning -= OnSecondOxygenWarning;
            }
        }

        private void RegisterOxygenWarning(object sender, Player.PlayerIdEventArgs e)
        {
            if (NetworkManager.LocalClientId != e.PlayerId) return;

            Player.Instance.PlayerOxygenController.OnFirstOxygenWarning += OnFirstOxygenWarning;
            Player.Instance.PlayerOxygenController.OnSecondOxygenWarning += OnSecondOxygenWarning;
        }

        private void OnFirstOxygenWarning()
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.OxygenWarningSFX, default);

            _firstWarningPanel.gameObject.SetActive(true);
            StartCoroutine(DisableWarningPanelAfterDelay(_firstDisplayTimer));
        }

        private void OnSecondOxygenWarning()
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.OxygenWarningSFX, default);

            EnableSecondWarningPanel();
            StopAllCoroutines();
            StartCoroutine(DisableWarningPanelAfterDelay(_secondDisplayTimer));
        }

        private IEnumerator DisableWarningPanelAfterDelay(float timer)
        {
            yield return new WaitForSeconds(timer);
            DisableWarningPanel();
        }

        private void EnableSecondWarningPanel()
        {
            _secondWarningPanel.gameObject.SetActive(true);
            _secondWarningPanel.localScale = Vector3.one;
            _secondWarningPanel.DOKill();

            Sequence pulseSequence = DOTween.Sequence();
            
            for (int i = 0; i < _pulseCount; i++)
            {
                pulseSequence.Append(_secondWarningPanel.DOScale(_pulseScale, _pulseDuration * 0.5f).SetEase(Ease.InOutSine));
                pulseSequence.Append(_secondWarningPanel.DOScale(1f, _pulseDuration * 0.5f).SetEase(Ease.InOutSine));
            }
            
            pulseSequence.Play();
        }

        private void DisableWarningPanel()
        {
            _secondWarningPanel.DOKill();
            _secondWarningPanel.gameObject.SetActive(false);
            _firstWarningPanel.gameObject.SetActive(false);
        }
    }
}