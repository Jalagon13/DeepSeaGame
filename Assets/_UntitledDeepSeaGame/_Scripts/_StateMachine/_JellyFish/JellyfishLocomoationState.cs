using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishLocomotionState : BaseState
    {
        private readonly JellyfishStateMachine _jellyContext;

        public JellyfishLocomotionState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true;
            _jellyContext = context as JellyfishStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            SetSubState(AIState.Moving);
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            
        }

        public override void CheckSwitchStates()
        {
            if (_jellyContext.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked, 0));
                return;
            }

            bool canSeePlayer = false;
            if (_jellyContext.NearestPlayer != null)
            {
                Vector2 myPos = _jellyContext.ServerCharacter.transform.position;
                Vector2 targetPos = _jellyContext.NearestPlayer.transform.position;
                if (_jellyContext.HasLineOfSight(myPos, targetPos))
                {
                    canSeePlayer = true;
                }
            }

            if (canSeePlayer && CurrentSubState.StateKey != AIState.Pursuing)
            {
                SetSubState(AIState.Pursuing);
            }
            else if (!canSeePlayer && CurrentSubState.StateKey != AIState.Moving)
            {
                SetSubState(AIState.Moving);
            }
        }
    }
}