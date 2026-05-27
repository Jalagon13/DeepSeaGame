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

        private Vector2 _moveInput;

        private Vector2 _desiredDirection;
        public Vector2 DesiredDirection => _desiredDirection;

        private Vector2 _velocity;
        public Vector2 Velocity => _velocity;

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
            
            if (_serverCharacter.CharacterData.CanMove)
            {
                // float currentSpeed = _serverCharacter.Stats.MovementSpeed.GetValue();
                float currentSpeed = _serverCharacter.CharacterData.BaseSpeed;

                // if (_serverCharacter.MovementState.Value == MovementState.Fleeing)
                // {
                //     currentSpeed *= _serverCharacter.CharacterData.FleeSpeedMultiplier;
                // }
                // else if (_serverCharacter.MovementState.Value == MovementState.Pursuing)
                // {
                //     currentSpeed *= _serverCharacter.CharacterData.PursueSpeedMultiplier;
                //     currentSpeed *= _strafeSpeedMultiplier;
                // }

                if (_serverCharacter.MovementState.Value == MovementState.Idle)
                {
                    _desiredDirection = Vector2.zero;
                }

                _velocity = Vector2.Lerp(_velocity, _desiredDirection * currentSpeed, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);
            }

            if (_desiredDirection != Vector2.zero)
            {
                _serverCharacter.CardinalDirection.Value = GetCardinalDirectionFromVector2(_desiredDirection);
            }
            
            _rigidbody2D.linearVelocity = _velocity;
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
