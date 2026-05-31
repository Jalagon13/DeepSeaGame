using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{

    // NTFS: Create separate Player and Npc server character movement scripts later
    public class ServerCharacterMovement : MonoBehaviour
    {
        [SerializeField]
        private ServerCharacter _serverCharacter;

        [SerializeField]
        private Rigidbody2D _rigidbody2D;
        public Rigidbody2D RigidBody2D => _rigidbody2D;
        
        [SerializeField] 
        private BoxCollider2D _feetCollider;

        private Vector2 _moveInput;

        private Vector2 _desiredDirection;
        public Vector2 DesiredDirection => _desiredDirection;

        private Vector2 _velocity;
        public Vector2 Velocity => _velocity;

        [Header("Air Movement Settings")]
        [SerializeField] private float _gravity = -30f;
        [SerializeField] private float _jumpPower = 12f;
        [SerializeField] private float _groundCheckDistance = 0.6f;
        [SerializeField] private LayerMask _groundLayer;

        private bool _isGrounded;
        private bool _jumpRequested;
        private RaycastHit2D _groundHit;

        public void FixedUpdateMovement()
        {
            if (WorldManager.Instance != null && !WorldManager.Instance.IsWorldReady)
            {
                _desiredDirection = Vector2.zero;
                _velocity = Vector2.zero;
                _rigidbody2D.linearVelocity = Vector2.zero;
                return;
            }

            if(Player.Instance.Character.LifeState == LifeState.Dead)
            {
                return;
            }
            
            if(_serverCharacter.CurrentEnvironment.Value == Environment.Water)
            {
                WaterMovement();
            }
            else
            {
                AirMovement();
            }
            
            _rigidbody2D.linearVelocity = _velocity;
        }

        public void StartKnockback(Vector2 knockerPosition, float knockbackForce, bool inverse = false)
        {
            Vector2 knockbackDirection = ((Vector2)_serverCharacter.transform.position - knockerPosition).normalized;
            _serverCharacter.MovementState.Value = MovementState.Knockback;
            // _knockback.ApplyKnockback(knockerPosition, knockbackForce, inverse);
        }

        private void WaterMovement()
        {
            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // In water mode, we treat the Jump button as a vertical 'Up' input override.
                // We use an effective input vector so we don't overwrite the cached _moveInput.
                Vector2 effectiveInput = _moveInput;
                if (GameInput.Instance.JumpHeldDown)
                {
                    effectiveInput.y = 1f;
                }

                // Re-evaluate movement state and direction based on the combined input
                if (effectiveInput.sqrMagnitude > 0.0001f)
                {
                    _desiredDirection = effectiveInput.normalized;
                }
                else
                {
                    _desiredDirection = Vector2.zero;
                }

                _velocity = Vector2.Lerp(_rigidbody2D.linearVelocity, _desiredDirection * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);
            }

            if (_desiredDirection != Vector2.zero)
            {
                _serverCharacter.CardinalDirection.Value = GetCardinalDirectionFromVector2(_desiredDirection);
            }
        }

        private void AirMovement()
        {
            // Debug.Log($"Air Movement");
            // 1. Grounded Check
            _isGrounded = IsGrounded();
            
            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;
                Vector2 currentVelocity = _rigidbody2D.linearVelocity;

                // 2. Horizontal Movement (Lerp for that snappy control)
                float targetX = Mathf.Lerp(currentVelocity.x, _desiredDirection.x * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);

                // 3. Vertical Movement (Constant Gravity)
                float targetY = currentVelocity.y + (_gravity * Time.fixedDeltaTime);

                // 4. Jump Logic
                if (_jumpRequested)
                {
                    if (_isGrounded)
                    {
                        targetY = _jumpPower;
                    }
                    _jumpRequested = false; // Consume request regardless of success
                }

                _velocity = new Vector2(targetX, targetY);

                // 5. Update Direction (Horizontal only in air)
                if (Mathf.Abs(_desiredDirection.x) > 0.01f)
                {
                    _serverCharacter.CardinalDirection.Value = _desiredDirection.x > 0 ? Direction.Right : Direction.Left;
                }
            }
        }

        private bool IsGrounded()
        {
            Vector2 boxCastOrigin = new(_feetCollider.bounds.center.x, _feetCollider.bounds.center.y);
            Vector2 boxCastSize = new(_feetCollider.bounds.size.x, _groundCheckDistance);
            
            _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _groundCheckDistance, _groundLayer);
            if(_groundHit.collider != null)
            {
                return true;
            }
            else
            {
                return false;
            }        
        }

        public Direction GetCardinalDirectionFromVector2(Vector2 desiredDirection)
        {
            if (Math.Abs(desiredDirection.x) > Math.Abs(desiredDirection.y))
            {
                return desiredDirection.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return desiredDirection.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        public void ReceiveMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;

            if (_moveInput.sqrMagnitude > 0.0001f)
            {
                StartMovement();
            }
            else
            {
                StartIdle();
            }
        }

        public void ReceiveJumpInput()
        {
            _jumpRequested = true;
        }

        public void StartMovement()
        {
            if (_serverCharacter == null)
            {
                return;
            }
            
            _desiredDirection = _moveInput.normalized;

            if (_serverCharacter.MovementState.Value != MovementState.Moving)
            {
                _serverCharacter.MovementState.Value = MovementState.Moving;
            }
        }

        public void StartIdle()
        {
            if (_serverCharacter == null)
            {
                return;
            }
            
            _desiredDirection = Vector2.zero;

            if (_serverCharacter.MovementState.Value != MovementState.Idle)
            {
                _serverCharacter.MovementState.Value = MovementState.Idle;
            }
        }
    }
}
