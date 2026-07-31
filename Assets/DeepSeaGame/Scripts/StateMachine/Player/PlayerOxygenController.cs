using System;
using Unity.Netcode;
using UnityEngine;

namespace DeepSeaGame
{
    public enum OxygenState
    {
        Full,
        Depleting,
        Empty,
        Refilling
    }

    public class PlayerOxygenController : NetworkBehaviour
    {
        public Action OnOxygenWarning;
    
        [SerializeField]
        private PlayerCharacterSO _playerSO;

        [SerializeField]
        private Transform _headPoint;
        
        [Header("Oxygen Warning")]
        
        [SerializeField]
        [Range(0f, 1f)]
        private float _oxygenWarningThreshold = 0.25f;

        private ServerCharacter _serverCharacter;
        private bool _hasTriggeredOxygenWarning;
        private OxygenTankItemSO _equippedOxygenTank;
        public OxygenTankItemSO EquippedOxygenTank => _equippedOxygenTank;
        public float MaxOxygenCapacity => GetMaxOxygenCapacity();

        public NetworkVariable<OxygenState> StateOfOxygen { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> CurrentOxygen { get; private set; } = new(default, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
        private float _drowningTimer;


        private void Awake()
        {
            _serverCharacter = GetComponent<ServerCharacter>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                CurrentOxygen.Value = GetMaxOxygenCapacity();
                StateOfOxygen.Value = OxygenState.Full;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (Player.Instance.Character.LifeState == LifeState.Dead) return;

            Status env = _serverCharacter.CurrentStatus.Value;

            Vector2Int headGridPos = new(Mathf.FloorToInt(_headPoint.position.x), Mathf.FloorToInt(_headPoint.position.y + 1));
            bool headInAir = !WorldManager.Instance.WorldDataStore.IsBelowSeaLevel(headGridPos.y) || WorldManager.Instance.WorldDataStore.IsUnderwaterAirAt(headGridPos.x, headGridPos.y);

            if (headInAir || env == Status.InAir)
            {
                HandleAirOxygen();
            }
            else if (env == Status.InWater)
            {
                HandleWaterOxygen();
            }

            if (StateOfOxygen.Value == OxygenState.Empty)
            {
                _drowningTimer += Time.deltaTime;
                if (_drowningTimer >= _playerSO.OxygenDepletedTimeBetweenDamage)
                {
                    _drowningTimer = 0f;
                    _serverCharacter.DamageReceiver.ReceiveHP(_serverCharacter, -_playerSO.OxygenDepletedDamage, false);
                }
            }
            else
            {
                _drowningTimer = 0f;
            }
        }

        public void OnRespawn()
        {
            CurrentOxygen.Value = GetMaxOxygenCapacity();
            StateOfOxygen.Value = OxygenState.Full;
        }

        public void EquipOxygenTank(OxygenTankItemSO oxygenTank)
        {
            if (!IsOwner) return;

            float previousCapacity = GetMaxOxygenCapacity();
            _equippedOxygenTank = oxygenTank;
            RecalculateOxygenCapacity(previousCapacity);
        }

        public void UnequipOxygenTank()
        {
            if (!IsOwner) return;

            float previousCapacity = GetMaxOxygenCapacity();
            _equippedOxygenTank = null;
            RecalculateOxygenCapacity(previousCapacity);
        }

        private void RecalculateOxygenCapacity(float previousCapacity)
        {
            float newCapacity = GetMaxOxygenCapacity();
            if (newCapacity <= 0f)
            {
                CurrentOxygen.Value = 0f;
                return;
            }

            if (previousCapacity <= 0f || CurrentOxygen.Value <= 0f)
            {
                CurrentOxygen.Value = Mathf.Min(CurrentOxygen.Value, newCapacity);
                return;
            }

            float remainingRatio = Mathf.Clamp01(CurrentOxygen.Value / previousCapacity);
            CurrentOxygen.Value = remainingRatio * newCapacity;
            CurrentOxygen.Value = Mathf.Min(CurrentOxygen.Value, newCapacity);
        }

        private void HandleWaterOxygen()
        {
            if (CurrentOxygen.Value > 0)
            {
                StateOfOxygen.Value = OxygenState.Depleting;
                CurrentOxygen.Value -= Time.deltaTime;

                float oxygenRatio = CurrentOxygen.Value / GetMaxOxygenCapacity();
                if (oxygenRatio <= _oxygenWarningThreshold && !_hasTriggeredOxygenWarning)
                {
                    _hasTriggeredOxygenWarning = true;
                    OnOxygenWarning?.Invoke();
                }

                if (CurrentOxygen.Value <= 0)
                {
                    CurrentOxygen.Value = 0;
                    StateOfOxygen.Value = OxygenState.Empty;
                }
            }
            else
            {
                StateOfOxygen.Value = OxygenState.Empty;
            }
        }

        private void HandleAirOxygen()
        {
            float maxCapacity = GetMaxOxygenCapacity();
            if (CurrentOxygen.Value < maxCapacity)
            {
                bool wasRefilling = StateOfOxygen.Value == OxygenState.Refilling;
                StateOfOxygen.Value = OxygenState.Refilling;

                if (!wasRefilling)
                {
                    AudioManager.Instance.PlayOneShot(FMODEvents.Instance.OxygenReplenishSFX, default);
                }

                // Total Capacity / Seconds to Refill = Units per second
                float refillRate = maxCapacity / _playerSO.OxygenRefillDuration;
                CurrentOxygen.Value += refillRate * Time.deltaTime;

                if (CurrentOxygen.Value >= maxCapacity)
                {
                    CurrentOxygen.Value = maxCapacity;
                    StateOfOxygen.Value = OxygenState.Full;
                }
            }
            else
            {
                StateOfOxygen.Value = OxygenState.Full;
            }

            // Reset the oxygen warning flag when oxygen is above the threshold
            float oxygenRatio = CurrentOxygen.Value / maxCapacity;
            if (oxygenRatio > _oxygenWarningThreshold)
            {
                _hasTriggeredOxygenWarning = false;
            }
        }

        private float GetMaxOxygenCapacity()
        {
            if (_playerSO == null)
            {
                return 0f;
            }

            int extraOxygen = _equippedOxygenTank != null ? _equippedOxygenTank.AdditionalOxygen : 0;
            return _playerSO.BaseOxygenDuration + extraOxygen;
        }
    }
}
