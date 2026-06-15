using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class JellyfishCharacterMovement : CharacterMovement
    {
        [Header("Jellyfish Movement Settings")]
        [SerializeField] private float _propulsionPower = 10f;
    
        private bool _isPropelling;
        public bool IsPropelling => _isPropelling;
    
        protected override void AirMovement()
        {
            // Apply gravity to vertical velocity
            float targetY = _velocity.y + (_gravity * Time.fixedDeltaTime);
            targetY = Mathf.Max(targetY, _terminalVelocity);
            _velocity = new Vector2(_velocity.x, targetY);
        }

        protected override void WaterMovement()
        {
            if(_isPropelling)
            {
                _velocity = Vector2.Lerp(_velocity, Vector2.zero, _serverCharacter.CharacterData.TurnSharpness * Time.fixedDeltaTime);
                
                if(_velocity.sqrMagnitude < 0.01f)
                {
                    _isPropelling = false;
                    _velocity = Vector2.zero;
                    Debug.Log($"Propulsion Ended");
                }
            }
        }
        
        public void StartPropulsion(Vector2 direction)
        {
            Debug.Log($"Propulsion Started");
            _isPropelling = true;
            DesiredDirection = direction;
            _velocity = direction * _propulsionPower;
        }
    }
}
