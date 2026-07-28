using UnityEngine;

namespace DeepSeaGame
{
    public class PlayerKnockbackState : BaseState
    {

        public PlayerKnockbackState(AIState key, StateMachine context) : base(key, context)
        {
            
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log("Player entering knockbacked");
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

        public override void ExitState()
        {

        }

        public override void ClientEnterState(AIStateData stateData)
        {
            // NTFS: Maybe add client side wind particles here
        }
    }
}
