using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class JellyfishMoveState : BaseState
    {
        private JellyfishStateMachine _ctx;
        private JellyfishCharacterMovement _jellyfishMovement;
        
        public JellyfishMoveState(AIState key, StateMachine context) : base(key, context)
        {
            _ctx = Context as JellyfishStateMachine;

            _jellyfishMovement = _ctx.ServerCharacter.Movement as JellyfishCharacterMovement;
            if (_jellyfishMovement == null)
            {
                Debug.LogError($"Jellyfish movement script could not be found.");
            }
        }

        protected override void EnterState(AIStateData stateData)
        {
            Debug.Log($"Jellyfish Move State Entered");
            Vector2 direction = GetDirection();
            _jellyfishMovement.StartPropulsion(direction);
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
            else if(!_jellyfishMovement.IsPropelling || _ctx.ServerCharacter.CurrentEnvironment.Value == Environment.Air)
            {
                SwitchState(new AIStateData(AIState.Idle));
                return;
            }
        }
    }
}