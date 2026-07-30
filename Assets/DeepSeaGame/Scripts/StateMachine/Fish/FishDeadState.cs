using UnityEngine;


namespace DeepSeaGame
{
    public class FishDeadState : BaseState
    {
        private FishStateMachine _ctx;
    
        public FishDeadState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true; // This is a super state
            _ctx = Context as FishStateMachine;
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

        public override void ClientEnterState(AIStateData stateData)
        {
            Debug.Log($"Entering DeadState");
            // NTFS: Player death animations here, just turn off visuals for now
            _ctx.ServerCharacter.ClientFeedbacks.PlayDeathFeedbacksRpc(stateData.Payload);
            _ctx.ServerCharacter.ClientCharacter.Visuals.SetActive(false);
        }

        public override void ClientExitState(AIStateData stateData)
        {
            _ctx.ServerCharacter.ClientCharacter.Visuals.SetActive(true);
        }
    }
}