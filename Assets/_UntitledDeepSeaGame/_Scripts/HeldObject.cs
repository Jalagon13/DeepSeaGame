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
        protected bool _isAttacking;

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

        public virtual void OnStart(ToolItemSO toolItem = null, bool isAttacking = false)
        {
            _currentToolItem = toolItem;
            _isAttacking = isAttacking;

            if (_isAttacking)
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
            _isAttacking = false;
        }

        public virtual void ExecuteAttack(PlayerArmController armController, Direction swingDir, Direction facingDir, ToolItemSO tool, System.Action onComplete)
        {
            // Default implementation just completes immediately.
            onComplete?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only process damage on the client who owns the player character during a swing
            if (!_isAttacking || _playerServerCharacter == null || !_playerServerCharacter.IsOwner)
            {
                return;
            }

            // Find ServerCharacter on the hit object, supporting child colliders
            ServerCharacter targetCharacter = collision.GetComponentInParent<ServerCharacter>();
            
            if (targetCharacter == null || targetCharacter.CharacterData == null || !targetCharacter.CharacterData.IsNpc)
            {
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
