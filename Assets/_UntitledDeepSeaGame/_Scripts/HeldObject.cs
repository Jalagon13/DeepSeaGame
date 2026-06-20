using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class HeldObject : MonoBehaviour
    {
        [SerializeField] private Collider2D _collider;
        private List<DamageReceiver> _targetsHit = new();
        private ServerCharacter _playerServerCharacter;
        private ToolItemSO _currentToolItem;
        private bool _isSwinging;

        protected virtual void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
                if (_collider == null)
                {
                    _collider = GetComponentInChildren<Collider2D>();
                }
            }

            if (_collider != null)
            {
                _collider.enabled = false;
                _collider.isTrigger = true; // Ensure it operates as a trigger
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] HeldObject: No Collider2D component was found on this object or its children!");
            }

            _playerServerCharacter = GetComponentInParent<ServerCharacter>();
            if (_playerServerCharacter == null)
            {
                Debug.LogError($"[{gameObject.name}] HeldObject: Could not find parent ServerCharacter component!");
            }
        }

        public virtual void OnStart(ToolItemSO toolItem = null, bool isSwinging = false)
        {
            _currentToolItem = toolItem;
            _isSwinging = isSwinging;

            if (_isSwinging)
            {
                _targetsHit.Clear();
                if (_collider != null)
                {
                    _collider.enabled = true;
                }
            }
        }
        
        public virtual void OnEnd()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
            _targetsHit.Clear();
            _isSwinging = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only process damage on the client who owns the player character during a swing
            if (!_isSwinging)
            {
                Debug.Log($"[{gameObject.name}] Trigger entered but not swinging, ignoring collision with: {collision.gameObject.name}");
                return;
            }

            if (_playerServerCharacter == null)
            {
                Debug.LogWarning($"[{gameObject.name}] OnTriggerEnter2D ignored: player ServerCharacter is null!");
                return;
            }

            if (!_playerServerCharacter.IsOwner)
            {
                Debug.Log($"[{gameObject.name}] Trigger entered but player is not owner, ignoring collision with: {collision.gameObject.name}");
                return; // Only owner client runs hit detection
            }

            // Find ServerCharacter on the hit object, supporting child colliders
            ServerCharacter targetCharacter = collision.GetComponentInParent<ServerCharacter>();
            if (targetCharacter == null)
            {
                Debug.Log($"[{gameObject.name}] Trigger overlapped with non-character object: {collision.gameObject.name}");
                return;
            }

            if (targetCharacter.CharacterData == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Overlapped character has no CharacterData: {targetCharacter.gameObject.name}");
                return;
            }

            if (!targetCharacter.CharacterData.IsNpc)
            {
                // Hit another player or self
                return;
            }

            if (targetCharacter.DamageReceiver == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Overlapped NPC has no DamageReceiver: {targetCharacter.gameObject.name}");
                return;
            }

            DamageReceiver targetReceiver = targetCharacter.DamageReceiver;

            // Ensure NPC is alive and hasn't been damaged yet during this specific swing
            if (!_targetsHit.Contains(targetReceiver) && targetReceiver.IsAlive())
            {
                _targetsHit.Add(targetReceiver);

                int damage = _currentToolItem != null ? _currentToolItem.Damage : 0;
                int knockback = _currentToolItem != null ? _currentToolItem.Knockback : 0;

                // Apply damage (negative HP value) and pass knockback
                targetReceiver.ReceiveHP(_playerServerCharacter, -damage, true, knockback);
                Debug.Log($"[{gameObject.name}] Dealt {damage} damage and {knockback} knockback to NPC: {targetCharacter.name}");
            }
        }
    }
}
