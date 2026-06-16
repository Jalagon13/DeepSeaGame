using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public abstract class CharacterMovement : MonoBehaviour
    {
        public event Action<CollisionResult> CollisionDetected;
        
        [HideInInspector] public Vector2 DesiredDirection;
        
        [SerializeField] protected ServerCharacter _serverCharacter;

        [SerializeField] protected GridCollider _gridCollider;
        
        [Header("Air Movement Settings (Base)")]
        [SerializeField] protected float _gravity = -30f;
        [SerializeField] protected float _terminalVelocity = -50f;

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
            CollisionResult result = _gridCollider.Move(_velocity, Time.fixedDeltaTime);

            if (result.HitX || result.HitY)
            {
                CollisionDetected?.Invoke(result);
            }

            HandleCollision(result);
        }
        
        protected abstract void WaterMovement();
        protected abstract void AirMovement();

        protected virtual void HandleCollision(CollisionResult result)
        {
            // Default behavior is stop the velocity on collision
            if (result.HitX) _velocity.x = 0;
            if (result.HitY) _velocity.y = 0;
        }

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
            DesiredDirection = moveInput;

            if (DesiredDirection.sqrMagnitude > 0.0001f)
            {
                StartMovement();
            }
            else
            {
                StartIdle();
            }
        }

        public virtual void StartMovement()
        {
            if (_serverCharacter == null)
            {
                return;
            }

            DesiredDirection.Normalize();

            if (_serverCharacter.MovementState.Value != MovementState.Moving)
            {
                _serverCharacter.MovementState.Value = MovementState.Moving;
            }
        }

        public virtual void StartIdle()
        {
            if (_serverCharacter == null)
            {
                return;
            }

            DesiredDirection = Vector2.zero;

            if (_serverCharacter.MovementState.Value != MovementState.Idle)
            {
                _serverCharacter.MovementState.Value = MovementState.Idle;
            }
        }
    }
}
