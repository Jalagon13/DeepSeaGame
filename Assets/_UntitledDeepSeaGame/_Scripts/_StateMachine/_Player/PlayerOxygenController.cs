using System;
using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
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
        [SerializeField]
        private PlayerCharacterSO _playerSO;
        
        [SerializeField] 
        private Transform _headPoint;
        
        private ServerCharacter _serverCharacter;
        
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
                CurrentOxygen.Value = _playerSO.BaseOxygenDuration;
                StateOfOxygen.Value = OxygenState.Full;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            if(Player.Instance.Character.LifeState == LifeState.Dead) return;

            Status env = _serverCharacter.CurrentStatus.Value;

            Vector2Int headGridPos = new(Mathf.FloorToInt(_headPoint.position.x), Mathf.FloorToInt(_headPoint.position.y + 1));
            bool headInAir = WorldManager.Instance.WorldDataStore.IsAirAt(headGridPos.x, headGridPos.y);

            if(headInAir || env == Status.InAir)
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
            CurrentOxygen.Value = _playerSO.BaseOxygenDuration;
            StateOfOxygen.Value = OxygenState.Full;
        }

        private void HandleWaterOxygen()
        {
            if (CurrentOxygen.Value > 0)
            {
                StateOfOxygen.Value = OxygenState.Depleting;
                CurrentOxygen.Value -= Time.deltaTime;

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
            if (CurrentOxygen.Value < _playerSO.BaseOxygenDuration)
            {
                StateOfOxygen.Value = OxygenState.Refilling;

                // Total Capacity / Seconds to Refill = Units per second
                float refillRate = (float)_playerSO.BaseOxygenDuration / _playerSO.OxygenRefillDuration;
                CurrentOxygen.Value += refillRate * Time.deltaTime;

                if (CurrentOxygen.Value >= _playerSO.BaseOxygenDuration)
                {
                    CurrentOxygen.Value = _playerSO.BaseOxygenDuration;
                    StateOfOxygen.Value = OxygenState.Full;
                }
            }
            else
            {
                StateOfOxygen.Value = OxygenState.Full;
            }
        }


    }
}
