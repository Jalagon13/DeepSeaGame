using UnityEngine;


namespace UntitledDeepSeaGame
{
    public class FishDeadState : BaseState
    {
        public FishDeadState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true; // This is a super state
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

        }
    }
}