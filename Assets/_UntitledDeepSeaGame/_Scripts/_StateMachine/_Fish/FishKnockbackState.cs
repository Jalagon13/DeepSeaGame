using UnityEngine;

namespace DeepSeaGame
{
    public class FishKnockbackState : BaseState
    {

        public FishKnockbackState(AIState key, StateMachine context) : base(key, context)
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
            if (Context.ServerCharacter.MovementState.Value != MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Moving));
            }
        }
    }
}
