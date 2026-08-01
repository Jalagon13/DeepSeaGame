using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

namespace DeepSeaGame
{
    public class ThrustWeapon : HeldObject
    {
        [SerializeField] private Transform _thrustWeaponTip;
        private List<DamageReceiver> _targetsHit = new();

        public override void OnStart(ToolItemSO toolItem = null, bool isAttacking = false)
        {
            base.OnStart(toolItem, isAttacking);
            if (isAttacking)
            {
                _targetsHit.Clear();
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            _targetsHit.Clear();
        }

        public override void ExecuteAttack(PlayerArmController armController, Direction swingDir, Direction facingDir, ToolItemSO tool, Action onComplete)
        {
            Transform holder = transform.parent;
            Transform pivot = armController.HeldItemPivot;
            
            // Thrust logic:
            // Point the pivot towards the mouse based on AngleToMouse.
            float angle = armController.AngleToMouse.Value;
            pivot.rotation = Quaternion.Euler(0, 0, angle);

            Vector3 originalLocalPos = holder.localPosition;
            
            // Calculate the tip's local X distance relative to the holder
            float tipDistanceX = holder.InverseTransformPoint(_thrustWeaponTip.position).x;
            
            // Pull the holder back so the tip is at the holder's original position
            Vector3 pulledBackPos = originalLocalPos - new Vector3(tipDistanceX, 0, 0);
            
            // Snap to pulled back position immediately
            holder.localPosition = pulledBackPos;

            // Thrust outward by _thrustDistance from the pulled back position
            float targetX = pulledBackPos.x + tool.ThrustDistance;
            
            // Thrust out and back
            holder.DOLocalMoveX(targetX, tool.AttackDuration / 2f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => 
                {
                    // Restore original position
                    holder.localPosition = originalLocalPos;
                    onComplete?.Invoke();
                });
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
