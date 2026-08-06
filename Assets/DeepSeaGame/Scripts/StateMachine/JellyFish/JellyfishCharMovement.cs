using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishCharMovement : CharacterMovement
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private float _visualRotationSpeed = 8f;

        [Header("Jellyfish AI Settings")]
        public float PropelSpeed = 10f;
        public float WaitTimeAfterPropel = 2f;
        public float SeekRadius = 15f;
        public float SwimDrag = 2f;

        private Quaternion _currentVisualRotation = Quaternion.identity;

        protected override void AirMovement()
        {
            _velocity.y += _gravity * Time.fixedDeltaTime;
            if (_velocity.y < _gravity) _velocity.y = _gravity;
        }

        protected override void WaterMovement()
        {
            // Apply drag to gradually slow down after propelling
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, Time.fixedDeltaTime * SwimDrag);

            // Rotate character visuals so their y-axis points toward the velocity direction, lerping smoothly
            Quaternion targetVisualRotation = default;
            
            if (_serverCharacter.StateMachine.CurrentState.StateKey == AIState.Locomotion)
            {
                Vector3 velocityDirection = new Vector3(_velocity.x, _velocity.y, 0f).normalized;
                targetVisualRotation = Quaternion.FromToRotation(Vector3.up, velocityDirection);
            }

            _currentVisualRotation = Quaternion.Slerp(_currentVisualRotation, targetVisualRotation, _visualRotationSpeed * Time.fixedDeltaTime);
            _visuals.rotation = _currentVisualRotation;
        }

        public void Propel(Vector2 direction)
        {
            _velocity = direction.normalized * PropelSpeed;
        }

        protected override void HandleCollision(CollisionResult result)
        {
            // If we hit a wall/ceiling/floor, bounce off
            if (result.HitX) _velocity.x *= -1f;
            if (result.HitY) _velocity.y *= -1f;
        }
    }
}