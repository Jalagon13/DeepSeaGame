using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class JellyfishKnockbackState : BaseState
    {

        public JellyfishKnockbackState(AIState key, StateMachine context) : base(key, context)
        {

        }

        protected override void EnterState(AIStateData stateData)
        {

        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (Context.ServerCharacter.MovementState.Value == MovementState.Idle)
            {
                SwitchState(new AIStateData(AIState.Idle));
            }
            else if (Context.ServerCharacter.MovementState.Value == MovementState.Moving)
            {
                SwitchState(new AIStateData(AIState.Moving));
            }
        }
    }
}
