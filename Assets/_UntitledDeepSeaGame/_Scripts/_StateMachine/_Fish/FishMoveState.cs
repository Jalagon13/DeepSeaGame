using System;
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
            // Debug.Log($"Jellyfish Move State Entered");
            Vector2 direction = GetDirection();
            _fishMovement.StartPropulsion(direction);
        }

        private Vector2 GetDirection()
        {
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
            return randomDirection;
        }

        public override void ExitState()
        {

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
            else if(!_fishMovement.IsPropelling || _ctx.ServerCharacter.CurrentStatus.Value == Status.InAir)
            {
                SwitchState(new AIStateData(AIState.Idle));
                return;
            }
        }
    }
}