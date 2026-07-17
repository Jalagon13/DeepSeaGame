using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerGroundedState : BaseState
    {
        private PlayerStateMachine _ctx;

        public PlayerGroundedState(AIState key, StateMachine context) : base(key, context)
        {
            _ctx = Context as PlayerStateMachine;
            IsSuperState = true;
            SetSubState(AIState.Idle);
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log("Player entering grounded");

        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (!GameInput.Instance.IsGameplayInputBlocked)
            {
                if (_ctx.HeldItem is ToolItemSO tool && (GameInput.Instance.PrimaryActionHeldDown || GameInput.Instance.SecondaryActionHeldDown))
                {
                    if (tool.HarvestType == ToolType.Drill)
                    {
                        SwitchState(new AIStateData(AIState.Mining));
                    }
                    else if (tool.HarvestType == ToolType.Spear)
                    {
                        SwitchState(new AIStateData(AIState.Attacking));
                    }
                }
            }
            else if (_ctx.ServerCharacter.LifeState == LifeState.Dead)
            {
                Vector3 payload = new(_ctx.ServerCharacter.InflicterToTargetDirection.x, _ctx.ServerCharacter.InflicterToTargetDirection.y, _ctx.ServerCharacter.KnockbackForceFromInflicter);
                SwitchState(new AIStateData(AIState.Dead, payload));
            }
        }

        public override void ExitState()
        {

        }
    }
}