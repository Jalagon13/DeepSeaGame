using UnityEngine;
using DG.Tweening;
using System;

namespace UntitledDeepSeaGame
{
    public class ThrustWeapon : HeldObject
    {
        [SerializeField] private Transform _thrustWeaponTip;

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
    }
}
