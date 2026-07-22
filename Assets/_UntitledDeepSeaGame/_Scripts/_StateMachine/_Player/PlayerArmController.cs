using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DeepSeaGame
{
    public class PlayerArmController : NetworkBehaviour
    {
        public event Action<bool> AimingStateChanged;

        [SerializeField]
        private GameObject _heldItemPivot;
        [SerializeField]
        private GameObject _heldItemHolder;
        
        [Header("Pivots")]
        [SerializeField]
        private Transform _rightPivot;
        [SerializeField]
        private Transform _leftPivot;
        
        private HeldObject _currentHeldObject;
        private HeldObject _currentHeldPrefab;
        private ServerCharacter _serverCharacter;

        public bool IsAttacking { get; private set; }
        private bool _isAiming;
        public bool IsAiming => _isAiming;
        
        public NetworkVariable<Direction> AimDirection { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> AngleToMouse { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Direction> AttackDirection { get; private set; } = new(Direction.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public Transform HeldItemPivot => _heldItemPivot.transform;
        
        private void Awake()
        {
            _heldItemHolder.SetActive(false);
            _serverCharacter = GetComponent<ServerCharacter>();
        }

        private void Update()
        {
            if(IsOwner)
            {
                Vector3 direction = GameManager.MouseWorldPosition - (Vector2)_heldItemPivot.transform.position;
                AngleToMouse.Value = NormalizeAngle(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                AimDirection.Value = DetermineCardinalDirection(AngleToMouse.Value);
            }
            
            if(_isAiming)
            {
                _heldItemPivot.transform.rotation = Quaternion.AngleAxis(AngleToMouse.Value, Vector3.forward);
                SetPivotPosition(AimDirection.Value);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void StartAimHandRpc(ushort toolItemId)
        {
            ToolItemSO tool = GameDataRegistry.Instance.GetItemSOFromItemId(toolItemId) as ToolItemSO;
            EnsureCurrentHeldObject(tool.HeldObject);

            _isAiming = true;
            _heldItemHolder.SetActive(true);
            _currentHeldObject.OnStart(tool, false);
            AimingStateChanged?.Invoke(_isAiming);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void EndAimHandRpc()
        {
            _isAiming = false;
            _heldItemHolder.SetActive(false);
            _currentHeldObject.OnEnd();
            AimingStateChanged?.Invoke(_isAiming);
        }

        public void ExecuteAttack(ushort toolItemId)
        {
            if (IsAttacking) return;
            IsAttacking = true;

            float angle = AngleToMouse.Value;
            Direction facingDir;
            Direction swingDir;

            if (angle > 220f && angle < 320f)
            {
                swingDir = Direction.Down;
                facingDir = (angle > 90f && angle < 270f) ? Direction.Left : Direction.Right;
            }
            else if (angle >= 90f && angle <= 220f)
            {
                swingDir = Direction.Left;
                facingDir = Direction.Left;
            }
            else
            {
                swingDir = Direction.Right;
                facingDir = Direction.Right;
            }

            if (_serverCharacter != null)
            {
                _serverCharacter.CurrentDirection.Value = facingDir;
            }

            AttackDirection.Value = swingDir;

            PerformAttackClientRpc(swingDir, facingDir, toolItemId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PerformAttackClientRpc(Direction swingDir, Direction facingDir, ushort toolItemId)
        {
            IsAttacking = true;
            
            ToolItemSO toolItemSO = GameDataRegistry.Instance.GetItemSOFromItemId(toolItemId) as ToolItemSO;

            EnsureCurrentHeldObject(toolItemSO.HeldObject);
            SetPivotPosition(facingDir);

            _heldItemHolder.SetActive(true);
            _currentHeldObject.OnStart(toolItemSO, true);

            _currentHeldObject.ExecuteAttack(this, swingDir, facingDir, toolItemSO, () =>
            {
                _heldItemHolder.SetActive(false);
                _currentHeldObject.OnEnd();
                
                AttackDirection.Value = Direction.None;
                IsAttacking = false;
            });
        }

        private void EnsureCurrentHeldObject(HeldObject swingPrefab)
        {
            if (_currentHeldObject != null && _currentHeldPrefab == swingPrefab)
            {
                return;
            }

            if (_currentHeldObject != null)
            {
                Destroy(_currentHeldObject.gameObject);
            }

            _currentHeldPrefab = swingPrefab;
            _currentHeldObject = Instantiate(swingPrefab, _heldItemHolder.transform);
        }

        private void SetPivotPosition(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    _heldItemPivot.transform.position = _leftPivot.transform.position;
                    break;
                case Direction.Right:
                    _heldItemPivot.transform.position = _rightPivot.transform.position;
                    break;
            }
        }

        private float NormalizeAngle(float angle)
        {
            return (angle % 360 + 360) % 360;
        }

        private Direction DetermineCardinalDirection(float angle)
        {
            if (angle < 45 || angle > 315) return Direction.Right;
            if (angle < 135) return Direction.Up;
            if (angle < 225) return Direction.Left;
            return Direction.Down;
        }

        
    }
}
