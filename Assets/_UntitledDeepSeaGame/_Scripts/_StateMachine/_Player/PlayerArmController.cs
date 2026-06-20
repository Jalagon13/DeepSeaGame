using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerArmController : NetworkBehaviour
    {
        [SerializeField]
        private GameObject _heldItemPivot;
        [SerializeField]
        private GameObject _heldItemHolder;
        
        [Header("Pivots")]
        [SerializeField]
        private Transform _eastPivot;
        [SerializeField]
        private Transform _westPivot;
        
        public bool IsSwinging { get; private set; }
        
        public NetworkVariable<Direction> AimDirection { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> AngleToMouse { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Direction> SwingDirection { get; private set; } = new(Direction.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private HeldObject _currentHeldObject;
        private HeldObject _currentHeldPrefab;
        private bool _isAiming;
        
        private void Awake()
        {
            _heldItemHolder.SetActive(false);
        }

        private void Update()
        {
            if(IsOwner)
            {
                Vector3 direction = GameManager.MouseWorldPosition - (Vector2)transform.position;
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
            _currentHeldObject.OnStart();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void EndAimHandRpc()
        {
            _isAiming = false;
            _heldItemHolder.SetActive(false);
            _currentHeldObject.OnEnd();
        }

        public void PerformSwing(Quaternion startRotation, Quaternion endRotation, float duration, Direction swingDirection, ushort toolItemId)
        {
            SwingDirection.Value = swingDirection;

            PerformSwingClientRpc(startRotation, endRotation, duration, swingDirection, toolItemId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PerformSwingClientRpc(Quaternion startRotation, Quaternion endRotation, float duration, Direction direction, ushort toolItemId)
        {
            IsSwinging = true;
            
            ToolItemSO toolItemSO = GameDataRegistry.Instance.GetItemSOFromItemId(toolItemId) as ToolItemSO;

            EnsureCurrentHeldObject(toolItemSO.HeldObject);
            SetPivotPosition(direction);

            _heldItemPivot.transform.rotation = startRotation;
            _heldItemHolder.SetActive(true);
            _currentHeldObject.OnStart();

            _heldItemPivot.transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                _heldItemPivot.transform.rotation = endRotation;
                _heldItemHolder.SetActive(false);
                _currentHeldObject.OnEnd();
                
                SwingDirection.Value = Direction.None;
                IsSwinging = false;
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
                    _heldItemPivot.transform.position = _westPivot.transform.position;
                    break;
                case Direction.Right:
                    _heldItemPivot.transform.position = _eastPivot.transform.position;
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
