using System;
using UnityEngine;

namespace DeepSeaGame
{
    public class PlayerCharacterMovement : CharacterMovement
    {
        [Header("Air Movement Settings")]
        [SerializeField] private float _jumpPower = 12f; // This remains specific to the player's jump

        private bool _isGrounded;
        private bool _jumpRequested;
        private PlayerArmController _playerArmController;

        private void Awake()
        {
            _playerArmController = GetComponent<PlayerArmController>();
        }

        protected override void WaterMovement()
        {
            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // In water mode, we treat the Jump button as a vertical 'Up' input override.
                // We use an effective input vector so we don't overwrite the cached _desiredDirection.
                Vector2 effectiveInput = DesiredDirection;
                if (GameInput.Instance.JumpHeldDown)
                {
                    effectiveInput.y = 1f;
                }

                // Re-evaluate movement state and direction based on the combined input
                if (effectiveInput.sqrMagnitude > 0.0001f)
                {
                    DesiredDirection = effectiveInput.normalized;
                }
                else
                {
                    DesiredDirection = Vector2.zero;
                }

                _velocity = Vector2.Lerp(_velocity, DesiredDirection * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);
            }

            // Only allow changing facing direction if we are not currently swinging
            if (_playerArmController == null || !_playerArmController.IsAttacking)
            {
                if (Mathf.Abs(DesiredDirection.x) > 0.01f)
                {
                    _serverCharacter.CurrentDirection.Value = DesiredDirection.x > 0 ? Direction.Right : Direction.Left;
                }
            }
        }

        protected override void AirMovement()
        {
            // 1. Grounded Check via our Grid Data
            _isGrounded = _gridCollider.IsGrounded();

            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // 2. Horizontal Movement (Lerp for that snappy control)
                float targetX = Mathf.Lerp(_velocity.x, DesiredDirection.x * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);

                // 3. Vertical Movement (Constant Gravity)
                float targetY = _velocity.y;
                if (!_isGrounded || targetY > 0)
                {
                    targetY += (_gravity * Time.fixedDeltaTime);
                    targetY = Mathf.Max(targetY, _terminalVelocity);
                }
                else
                {
                    targetY = 0f;
                }

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

                // 5. Update Direction (Horizontal only in air, only if not swinging)
                if (_playerArmController == null || !_playerArmController.IsAttacking)
                {
                    if (Mathf.Abs(DesiredDirection.x) > 0.01f)
                    {
                        _serverCharacter.CurrentDirection.Value = DesiredDirection.x > 0 ? Direction.Right : Direction.Left;
                    }
                }
            }
        }

        public void ReceiveJumpInput()
        {
            _jumpRequested = true;
        }

    }
}
