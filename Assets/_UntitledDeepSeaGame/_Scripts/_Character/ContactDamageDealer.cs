using Unity.Netcode;
using UnityEngine;

namespace DeepSeaGame
{
    public class ContactDamageDealer : NetworkBehaviour
    {
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

            ServerCharacter target = collision.transform.root.GetComponent<ServerCharacter>();
            if (target == null || target == _source || target.CharacterData == null || target.CharacterData.IsNpc || target.GridCollider.BodyCollider != collision)
            {
                return;
            }

            DamageReceiver damageReceiver = target.DamageReceiver;
            if (damageReceiver == null || !damageReceiver.IsAlive())
            {
                return;
            }
            Debug.Log($"[{_source.name}] ContactDamageDealer: Dealing {_source.CharacterData.Damage} damage to [{target.name}]");
            damageReceiver.ReceiveHP(_source, -_source.CharacterData.Damage, _source.CharacterData.PlayKnockback, _source.CharacterData.KnockbackForce);
        }
    }
}
