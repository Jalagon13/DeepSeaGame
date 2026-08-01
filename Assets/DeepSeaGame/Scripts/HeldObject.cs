using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    public class HeldObject : MonoBehaviour
    {
        [SerializeField] private Collider2D _collider;
        protected ServerCharacter _playerServerCharacter;
        protected ToolItemSO _currentToolItem;
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
            _isAttacking = false;
        }

        public virtual void ExecuteAttack(PlayerArmController armController, Direction swingDir, Direction facingDir, ToolItemSO tool, System.Action onComplete)
        {
            // Default implementation just completes immediately.
            onComplete?.Invoke();
        }
    }
}
