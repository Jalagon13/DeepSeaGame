using System;
using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerAttackState : BaseState
    {
        private PlayerStateMachine _ctx;
        private ToolItemSO _toolItemSO;

        public PlayerAttackState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true;
            _ctx = Context as PlayerStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            _toolItemSO = _ctx.HeldItem as ToolItemSO;
            if (_toolItemSO == null) return;

            ushort toolItemId = GameDataRegistry.Instance.GetItemIdFromItemSO(_toolItemSO);
            float duration = _toolItemSO.SwingDuration;

            Player.Instance.PlayerArmController.ExecuteSwing(toolItemId, duration);
        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (!Player.Instance.PlayerArmController.IsSwinging)
            {
                SwitchState(new AIStateData(AIState.Grounded, 0));
            }
        }

        public override void ExitState()
        {
            
        }
    }
}
