using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Unity.Services.Matchmaker.Models;

namespace DeepSeaGame
{
    public class DrillObject : HeldObject
    {
        [SerializeField] private SpriteRenderer _drillSr;
        [SerializeField] private float _damageInterval = 0.2f;
        [SerializeField] private float _vibrationAmount = 0.025f;

        private Dictionary<DamageReceiver, float> _damageCooldowns = new();
        private EventInstance _drillSoundEventInstance;
        private Coroutine _vibrateCoroutine;

        protected override void Awake() 
        {
            base.Awake();
            
            _drillSoundEventInstance = RuntimeManager.CreateInstance(FMODEvents.Instance.DrillSFX);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ProcessDamage(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            ProcessDamage(collision);
        }

        public override void OnStart(ToolItemSO toolItem = null, bool isAttacking = false)
        {
            // The drill is considered 'active' and dangerous as long as it's out.
            // We force isAttacking = true so the base class enables the collider and ProcessDamage passes the check.
            base.OnStart(toolItem, true);
            _damageCooldowns.Clear();
            _drillSoundEventInstance.start();

            if (_drillSr != null)
            {
                if (_vibrateCoroutine != null)
                    StopCoroutine(_vibrateCoroutine);
                    
                _vibrateCoroutine = StartCoroutine(VibrateDrillSR());
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _damageCooldowns.Clear();
            _drillSoundEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            if (_vibrateCoroutine != null)
            {
                StopCoroutine(_vibrateCoroutine);
                _vibrateCoroutine = null;
            }

            if (_drillSr != null)
                _drillSr.transform.localPosition = Vector3.zero;
        }

        private IEnumerator VibrateDrillSR()
        {
            while (true)
            {
                float x = Random.Range(-_vibrationAmount, _vibrationAmount);
                float y = Random.Range(-_vibrationAmount, _vibrationAmount);
                _drillSr.transform.localPosition = new Vector3(x, y, 0f);
                yield return null;
            }
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
