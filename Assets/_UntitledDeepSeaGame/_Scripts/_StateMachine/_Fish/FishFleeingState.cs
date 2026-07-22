using UnityEngine;

namespace DeepSeaGame
{
    public class FishFleeingState : BaseState
    {
        private readonly FishStateMachine _ctx;
        private readonly FishCharMovement _fishMovement;

        public FishFleeingState(AIState key, StateMachine context) : base(key, context)
        {
            _ctx = Context as FishStateMachine;

            _fishMovement = _ctx.ServerCharacter.Movement as FishCharMovement;
            if (_fishMovement == null)
            {
                Debug.LogError($"Fish movement script could not be found.");
            }
        }

        protected override void EnterState(AIStateData stateData)
        {
            _fishMovement.StartFleeing(_ctx.LatestAttacker);
        }

        public override void ExitState()
        {
            _fishMovement.StopFleeing();
        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (_ctx.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked));
                return;
            }

            if (_ctx.LatestAttacker == null || _fishMovement.IsFleeDistanceReached(_ctx.LatestAttacker))
            {
                _ctx.ClearLatestAttacker(_ctx.LatestAttacker);
                SwitchState(new AIStateData(AIState.Moving));
            }
        }
    }
}
