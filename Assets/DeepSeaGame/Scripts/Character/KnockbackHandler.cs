using UnityEngine;

namespace DeepSeaGame
{
    public class KnockbackHandler
    {
        private const float DefaultDecayMultiplier = 5f;
        private const float MinKnockbackForce = 0f;
        private const float MaxKnockbackForce = 100f;
        private const float EndThreshold = 1.5f;
        
        private ServerCharacter _serverCharacter;
        private float _decayMultiplier = DefaultDecayMultiplier;

        public Vector2 Velocity { get; private set; }
        public bool IsActive { get; private set; }
    
        public KnockbackHandler(ServerCharacter character)
        {
            _serverCharacter = character;
        }

        public void Tick(float fixedDeltaTime)
        {
            if (!IsActive) return;

            Velocity = Vector2.Lerp(Velocity, Vector2.zero, _decayMultiplier * fixedDeltaTime);

            if (Velocity.magnitude <= EndThreshold)
            {
                Stop();
            }
        }

        public void Apply(Vector2 sourcePosition, float knockbackForce, bool inverse = false)
        {
            if (_serverCharacter == null || !_serverCharacter.CharacterData.CanBeKnockedBack)
            {
                return;
            }

            if (knockbackForce <= 0f)
            {
                Stop();
                return;
            }

            Vector2 direction = ((Vector2)_serverCharacter.transform.position - sourcePosition).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.up;
            }

            if (inverse)
            {
                direction *= -1f;
            }

            float resistance = Mathf.Clamp01(_serverCharacter.CharacterData.KnockbackResist);
            float finalForce = Mathf.Clamp(knockbackForce * (1f - resistance), MinKnockbackForce, MaxKnockbackForce);

            if (finalForce <= 0f)
            {
                Stop();
                return;
            }

            _decayMultiplier = Mathf.Lerp(10f, 1f, finalForce / MaxKnockbackForce);
            Velocity = direction * finalForce;
            IsActive = true;
        }

        public void ApplyDirection(Vector2 direction, float knockbackForce)
        {
            if (_serverCharacter == null || !_serverCharacter.CharacterData.CanBeKnockedBack)
            {
                return;
            }

            if (direction.sqrMagnitude <= 0.0001f || knockbackForce <= 0f)
            {
                Stop();
                return;
            }

            float resistance = Mathf.Clamp01(_serverCharacter.CharacterData.KnockbackResist);
            float finalForce = Mathf.Clamp(knockbackForce * (1f - resistance), MinKnockbackForce, MaxKnockbackForce);

            if (finalForce <= 0f)
            {
                Stop();
                return;
            }

            _decayMultiplier = Mathf.Lerp(10f, 1f, finalForce / MaxKnockbackForce);
            Velocity = direction.normalized * finalForce;
            IsActive = true;
        }

        public void HandleCollision(CollisionResult result)
        {
            if (result.HitX)
            {
                Velocity = new Vector2(0f, Velocity.y);
            }

            if (result.HitY)
            {
                Velocity = new Vector2(Velocity.x, 0f);
            }

            if (Velocity.magnitude <= EndThreshold)
            {
                Stop();
            }
        }

        public void Stop()
        {
            Velocity = Vector2.zero;
            IsActive = false;
        }
    }
}
