using Unity.Netcode;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class ContactDamageDealer : NetworkBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private float _knockbackForce = 6f;
        [SerializeField] private bool _playKnockback = true;

        private ServerCharacter _source;

        private void Awake()
        {
            _source = GetComponentInParent<ServerCharacter>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryDamage(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryDamage(collision);
        }

        private void TryDamage(Collider2D collision)
        {
            if (!IsServer || _source == null)
            {
                return;
            }

            ServerCharacter target = collision.GetComponentInParent<ServerCharacter>();
            if (target == null || target == _source || target.CharacterData == null || target.CharacterData.IsNpc)
            {
                return;
            }

            DamageReceiver damageReceiver = target.DamageReceiver;
            if (damageReceiver == null || !damageReceiver.IsAlive())
            {
                return;
            }
            Debug.Log($"[{_source.name}] ContactDamageDealer: Dealing {_damage} damage to [{target.name}]");
            damageReceiver.ReceiveHP(_source, -_damage, _playKnockback, _knockbackForce);
        }
    }
}
