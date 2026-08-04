using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishCharMovement : CharacterMovement
    {
        [Header("Jellyfish AI Settings")]
        public float PropelSpeed = 10f;
        public float WaitTimeAfterPropel = 2f;
        public float SeekRadius = 15f;
        public float SwimDrag = 2f;

        protected override void AirMovement()
        {
            _velocity.y += _gravity * Time.fixedDeltaTime;
            if (_velocity.y < _gravity) _velocity.y = _gravity;
        }

        protected override void WaterMovement()
        {
            // Apply drag to gradually slow down after propelling
            _velocity = Vector2.Lerp(_velocity, Vector2.zero, Time.fixedDeltaTime * SwimDrag);
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