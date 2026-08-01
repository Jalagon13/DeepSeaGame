using UnityEngine;
using System.Collections.Generic;

namespace DeepSeaGame
{
    public class DrillObject : HeldObject
    {
        [SerializeField] private float _damageInterval = 0.2f;
        
        private Dictionary<DamageReceiver, float> _damageCooldowns = new Dictionary<DamageReceiver, float>();

        public override void OnStart(ToolItemSO toolItem = null, bool isAttacking = false)
        {
            // The drill is considered 'active' and dangerous as long as it's out.
            // We force isAttacking = true so the base class enables the collider and ProcessDamage passes the check.
            base.OnStart(toolItem, true);
            _damageCooldowns.Clear();
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _damageCooldowns.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ProcessDamage(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            ProcessDamage(collision);
        }

        private void ProcessDamage(Collider2D collision)
        {
            if (!_isAttacking || _playerServerCharacter == null || !_playerServerCharacter.IsOwner)
            {
                return;
            }

            ServerCharacter targetCharacter = collision.GetComponentInParent<ServerCharacter>();
            if (targetCharacter == null || targetCharacter.CharacterData == null || !targetCharacter.CharacterData.IsNpc)
            {
                return;
            }

            DamageReceiver targetReceiver = targetCharacter.DamageReceiver;

            if (targetReceiver != null && targetReceiver.IsAlive())
            {
                if (!_damageCooldowns.ContainsKey(targetReceiver) || _damageCooldowns[targetReceiver] <= Time.time)
                {
                    int damage = _currentToolItem != null ? _currentToolItem.Damage : 0;
                    int knockback = _currentToolItem != null ? _currentToolItem.Knockback : 0;

                    targetReceiver.ReceiveHP(_playerServerCharacter, -damage, true, knockback);
                    _damageCooldowns[targetReceiver] = Time.time + _damageInterval;
                }
            }
        }
    }
}
