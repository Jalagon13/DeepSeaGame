using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerCharacterMovement : CharacterMovement
    {
        [Header("Air Movement Settings")]
        [SerializeField] private float _jumpPower = 12f;
        [SerializeField] private float _gravity = -30f;
        [SerializeField] private float _terminalVelocity = -50f;

        private bool _isGrounded;
        private bool _jumpRequested;

        protected override void WaterMovement()
        {
            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // In water mode, we treat the Jump button as a vertical 'Up' input override.
                // We use an effective input vector so we don't overwrite the cached _desiredDirection.
                Vector2 effectiveInput = _desiredDirection;
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

                _velocity = Vector2.Lerp(_velocity, _desiredDirection * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);
            }

            if (_desiredDirection != Vector2.zero)
            {
                _serverCharacter.CurrentDirection.Value = GetCardinalDirectionFromVector2(_desiredDirection);
            }
        }

        protected override void AirMovement()
        {
            // Debug.Log($"Air Movement");
            // 1. Grounded Check via our Grid Data
            _isGrounded = _gridCollider.IsGrounded();
            
            if (_serverCharacter.CharacterData.CanMove)
            {
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // 2. Horizontal Movement (Lerp for that snappy control)
                float targetX = Mathf.Lerp(_velocity.x, _desiredDirection.x * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);

                // 3. Vertical Movement (Constant Gravity)
                float targetY = _velocity.y + (_gravity * Time.fixedDeltaTime);
                targetY = Mathf.Max(targetY, _terminalVelocity);

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
                    _serverCharacter.CurrentDirection.Value = _desiredDirection.x > 0 ? Direction.Right : Direction.Left;
                }
            }
        }

        public void ReceiveJumpInput()
        {
            _jumpRequested = true;
        }

    }
}
