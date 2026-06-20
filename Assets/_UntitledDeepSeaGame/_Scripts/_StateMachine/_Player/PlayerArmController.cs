using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

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
        private Transform _rightPivot;
        [SerializeField]
        private Transform _leftPivot;
        
        [Header("Swing Configuration")]
        [SerializeField] private SwingConfig _leftSwing = new SwingConfig { StartAngle = 110, EndAngle = 250 };
        [SerializeField] private SwingConfig _rightSwing = new SwingConfig { StartAngle = 70, EndAngle = 290 };
        [SerializeField] private SwingConfig _downSwing = new SwingConfig { StartAngle = 340, EndAngle = 200 };
        [Serializable]
        public struct SwingConfig
        {
            public int StartAngle;
            public int EndAngle;
        }

        private HeldObject _currentHeldObject;
        private HeldObject _currentHeldPrefab;
        private bool _isAiming;
        private ServerCharacter _serverCharacter;

        public bool IsSwinging { get; private set; }
        
        public NetworkVariable<Direction> AimDirection { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> AngleToMouse { get; private set; } = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Direction> SwingDirection { get; private set; } = new(Direction.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        private void Awake()
        {
            _heldItemHolder.SetActive(false);
            _serverCharacter = GetComponent<ServerCharacter>();
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
            _currentHeldObject.OnStart(tool, false);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void EndAimHandRpc()
        {
            _isAiming = false;
            _heldItemHolder.SetActive(false);
            _currentHeldObject.OnEnd();
        }

        public SwingConfig GetSwingConfig(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return _leftSwing;
                case Direction.Right:
                    return _rightSwing;
                case Direction.Down:
                    return _downSwing;
                default:
                    return _rightSwing;
            }
        }

        private int MirrorAngle(int angle)
        {
            return (180 - angle % 360 + 360) % 360;
        }

        public void ExecuteSwing(ushort toolItemId, float duration)
        {
            if (IsSwinging) return;
            IsSwinging = true;

            // 1. Determine swing direction & facing direction based on AngleToMouse
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

            // 2. Set the player's facing direction
            if (_serverCharacter != null)
            {
                _serverCharacter.CurrentDirection.Value = facingDir;
            }

            // 3. Set swing direction state
            SwingDirection.Value = swingDir;

            // 4. Retrieve swing config and determine start/end angles and clockwise rotation
            int startAngle;
            int endAngle;
            bool clockwise;

            if (swingDir == Direction.Down)
            {
                SwingConfig config = GetSwingConfig(Direction.Down);
                if (facingDir == Direction.Right)
                {
                    startAngle = config.StartAngle;
                    endAngle = config.EndAngle;
                    clockwise = true;
                }
                else
                {
                    startAngle = MirrorAngle(config.StartAngle);
                    endAngle = MirrorAngle(config.EndAngle);
                    clockwise = false;
                }
            }
            else
            {
                SwingConfig config = GetSwingConfig(swingDir);
                startAngle = config.StartAngle;
                endAngle = config.EndAngle;
                clockwise = (facingDir == Direction.Right);
            }

            if (clockwise && endAngle > startAngle) startAngle += 360;
            else if (!clockwise && startAngle > endAngle) endAngle += 360;

            Quaternion startRotation = Quaternion.Euler(0, 0, startAngle);
            Quaternion endRotation = Quaternion.Euler(0, 0, endAngle);

            // 5. Trigger the RPC (passing the facingDir so pivot is placed correctly)
            PerformSwingClientRpc(startRotation, endRotation, duration, facingDir, toolItemId);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void PerformSwingClientRpc(Quaternion startRotation, Quaternion endRotation, float duration, Direction facingDir, ushort toolItemId)
        {
            IsSwinging = true;
            
            ToolItemSO toolItemSO = GameDataRegistry.Instance.GetItemSOFromItemId(toolItemId) as ToolItemSO;

            EnsureCurrentHeldObject(toolItemSO.HeldObject);
            SetPivotPosition(facingDir);

            _heldItemPivot.transform.rotation = startRotation;
            _heldItemHolder.SetActive(true);
            _currentHeldObject.OnStart(toolItemSO, true);

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
