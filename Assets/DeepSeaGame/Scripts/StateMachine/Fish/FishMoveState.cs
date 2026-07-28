using UnityEngine;

namespace DeepSeaGame
{
    public class FishMoveState : BaseState
    {
        private readonly FishStateMachine _ctx;
        private readonly FishCharMovement _fishMovement;
        
        public FishMoveState(AIState key, StateMachine context) : base(key, context)
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
            _fishMovement.StartHorizontalSwim();
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (_ctx.LatestAttacker != null && !_fishMovement.IsFleeDistanceReached(_ctx.LatestAttacker))
            {
                SwitchState(new AIStateData(AIState.Fleeing));
                return;
            }

            if (_ctx.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked));
                return;
            }
        }
    }
}
