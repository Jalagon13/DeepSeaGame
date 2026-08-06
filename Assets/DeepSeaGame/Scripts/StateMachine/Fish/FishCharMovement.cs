using UnityEngine;

namespace DeepSeaGame
{
    public class FishCharMovement : CharacterMovement
    {
        [Header("Fish Movement Settings")]
        [SerializeField] private float _minMoveDuration = 2f;
        [SerializeField] private float _maxMoveDuration = 5f;
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2f;
        [SerializeField] private float _fleeDistance = 10f;
        [SerializeField] private float _fleeSpeed = 10f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private int _horizontalDirection;
        private float _directionChangeTimer;
        private float _bobTime;
        private ServerCharacter _fleeTarget;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                ChooseInitialDirection();
                ResetDirectionChangeTimer();
            }
        }

        protected override void AirMovement()
        {
            // Apply gravity to vertical velocity
            float targetY = _velocity.y + (_gravity * Time.fixedDeltaTime);
            targetY = Mathf.Max(targetY, _gravity);
            _velocity = new Vector2(/* _velocity.x */ 0, targetY);
        }

        protected override void WaterMovement()
        {
            if (!_serverCharacter.CharacterData.CanMove)
            {
                _velocity = Vector2.zero;
                return;
            }

            TickDirectionChangeTimer();
            _bobTime += Time.fixedDeltaTime;

            if (_fleeTarget != null)
            {
                DesiredDirection = ((Vector2)(transform.position - _fleeTarget.transform.position)).normalized;
                _horizontalDirection = DesiredDirection.x >= 0f ? 1 : -1;
            }
            else
            {
                DesiredDirection = new Vector2(_horizontalDirection, 0f);
            }

            float verticalBob = Mathf.Sin(_bobTime * _bobFrequency * Mathf.PI * 2f) * _bobAmplitude;
            _velocity = _fleeTarget != null ? DesiredDirection * _fleeSpeed : new Vector2(DesiredDirection.x * _serverCharacter.CharacterData.BaseSpeed, verticalBob);
            _serverCharacter.CurrentDirection.Value = _horizontalDirection > 0 ? Direction.Right : Direction.Left;
            
            // TEMP: Update the sprite orientation based on the current direction
            if (_horizontalDirection > 0)
            {
                _spriteRenderer.flipX = true;
            }
            else if (_horizontalDirection < 0)
            {
                _spriteRenderer.flipX = false;
            }
        }

        private void TickDirectionChangeTimer()
        {
            _directionChangeTimer -= Time.fixedDeltaTime;

            if (_directionChangeTimer > 0f)
            {
                return;
            }

            FlipDirection();
            ResetDirectionChangeTimer();
        }

        protected override void HandleCollision(CollisionResult result)
        {
            if (!result.HitX)
            {
                return;
            }

            if(_serverCharacter.CurrentStatus.Value == Status.InWater)
            {
                FlipDirection();
                _velocity = new Vector2(-_velocity.x, 0f);
                ResetDirectionChangeTimer();
            }
            
        }

        public void StartHorizontalSwim()
        {
            if (_horizontalDirection == 0)
            {
                ChooseInitialDirection();
            }

            ResetDirectionChangeTimer();
        }

        public void StartFleeing(ServerCharacter attacker)
        {
            _fleeTarget = attacker;
            _serverCharacter.MovementState.Value = MovementState.Fleeing;
        }

        public void StopFleeing()
        {
            _fleeTarget = null;
            StartHorizontalSwim();
            if (_serverCharacter.MovementState.Value != MovementState.Knockback)
            {
                _serverCharacter.MovementState.Value = MovementState.Moving;
            }
        }

        public bool IsFleeDistanceReached(ServerCharacter attacker)
        {
            if (attacker == null)
            {
                return true;
            }

            return Vector2.Distance(transform.position, attacker.transform.position) >= _fleeDistance;
        }

        private void FlipDirection()
        {
            _horizontalDirection = _horizontalDirection >= 0 ? -1 : 1;
            DesiredDirection = new Vector2(_horizontalDirection, 0f);
        }

        private void ResetDirectionChangeTimer()
        {
            _directionChangeTimer = Random.Range(_minMoveDuration, _maxMoveDuration);
        }

        private void ChooseInitialDirection()
        {
            _horizontalDirection = Random.value < 0.5f ? -1 : 1;
            DesiredDirection = new Vector2(_horizontalDirection, 0f);
        }
    }
}
