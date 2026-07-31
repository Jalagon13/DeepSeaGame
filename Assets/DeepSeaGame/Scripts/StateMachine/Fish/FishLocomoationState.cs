using UnityEngine;


namespace DeepSeaGame
{
    public class FishLocomoationState : BaseState
    {
        public FishLocomoationState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true; // This is a super state
            SetSubState(AIState.Moving); // Default sub state is idle
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
            if (Context.ServerCharacter.LifeState == LifeState.Dead)
            {
                Vector3 payload = new(Context.ServerCharacter.InflicterToTargetDirection.x, Context.ServerCharacter.InflicterToTargetDirection.y, Context.ServerCharacter.KnockbackForceFromInflicter);
                SwitchState(new AIStateData(AIState.Dead, payload));
            }
        }
    }
}