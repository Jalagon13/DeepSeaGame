using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public abstract class CharacterMovement : MonoBehaviour
    {
        [SerializeField]
        protected ServerCharacter _serverCharacter;

        [SerializeField]
        protected GridCollider _gridCollider;

        protected Vector2 _desiredDirection;
        public Vector2 DesiredDirection => _desiredDirection;

        protected Vector2 _velocity;
        public Vector2 Velocity => _velocity;

        public void FixedUpdateMovement()
        {
            if (WorldManager.Instance != null && !WorldManager.Instance.IsWorldReady)
            {
                return;
            }

            if (_serverCharacter.LifeState == LifeState.Dead)
            {
                return;
            }

            if (_serverCharacter.CurrentEnvironment.Value == Environment.Water)
            {
                WaterMovement();
            }
            else
            {
                AirMovement();
            }

            // Use our custom grid collider to move the character and update our velocity
            // based on any collisions (e.g. hitting a floor sets Y velocity to 0)
            _velocity = _gridCollider.Move(_velocity, Time.fixedDeltaTime);
        }
        
        protected abstract void WaterMovement();
        protected abstract void AirMovement();

        public void StartKnockback(Vector2 knockerPosition, float knockbackForce, bool inverse = false)
        {
            Vector2 knockbackDirection = ((Vector2)_serverCharacter.transform.position - knockerPosition).normalized;
            _serverCharacter.MovementState.Value = MovementState.Knockback;
            // _knockback.ApplyKnockback(knockerPosition, knockbackForce, inverse);
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
            _desiredDirection = moveInput;

            if (_desiredDirection.sqrMagnitude > 0.0001f)
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

            _desiredDirection.Normalize();

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
