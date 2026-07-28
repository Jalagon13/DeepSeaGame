using UnityEngine;
using DG.Tweening;
using System;

namespace DeepSeaGame
{
    public class SwingWeapon : HeldObject
    {
        [Header("Swing Configuration")]
        [SerializeField] private SwingConfig _leftSwing = new SwingConfig { StartAngle = 110, EndAngle = 250 };
        [SerializeField] private SwingConfig _rightSwing = new SwingConfig { StartAngle = 70, EndAngle = 290 };
        [SerializeField] private SwingConfig _downSwing = new SwingConfig { StartAngle = 340, EndAngle = 200 };

        [Serializable]
        public struct SwingConfig
        {
            public int StartAngle;
            public int EndAngle;
        }

        public override void ExecuteAttack(PlayerArmController armController, Direction swingDir, Direction facingDir, ToolItemSO tool, Action onComplete)
        {
            int startAngle;
            int endAngle;
            bool clockwise;

            if (swingDir == Direction.Down)
            {
                SwingConfig config = GetSwingConfig(Direction.Down);
                if (facingDir == Direction.Right)
                {
                    startAngle = config.StartAngle;
                    endAngle = config.EndAngle;
                    clockwise = true;
                }
                else
                {
                    startAngle = MirrorAngle(config.StartAngle);
                    endAngle = MirrorAngle(config.EndAngle);
                    clockwise = false;
                }
            }
            else
            {
                SwingConfig config = GetSwingConfig(swingDir);
                startAngle = config.StartAngle;
                endAngle = config.EndAngle;
                clockwise = (facingDir == Direction.Right);
            }

            if (clockwise && endAngle > startAngle) startAngle += 360;
            else if (!clockwise && startAngle > endAngle) endAngle += 360;

            Quaternion startRotation = Quaternion.Euler(0, 0, startAngle);
            Quaternion endRotation = Quaternion.Euler(0, 0, endAngle);

            Transform pivot = armController.HeldItemPivot;
            pivot.rotation = startRotation;
            
            pivot.DORotateQuaternion(endRotation, tool.AttackDuration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                pivot.rotation = endRotation;
                onComplete?.Invoke();
            });
        }

        private int MirrorAngle(int angle)
        {
            return (180 - angle % 360 + 360) % 360;
        }

        public SwingConfig GetSwingConfig(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return _leftSwing;
                case Direction.Right:
                    return _rightSwing;
                case Direction.Down:
                    return _downSwing;
                default:
                    return _rightSwing;
            }
        }
    }
}
