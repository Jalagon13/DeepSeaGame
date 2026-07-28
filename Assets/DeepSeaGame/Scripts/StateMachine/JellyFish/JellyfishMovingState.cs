using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishMovingState : BaseState
    {
        private readonly JellyfishStateMachine _jellyContext;
        private float _timer;

        public JellyfishMovingState(AIState key, StateMachine context) : base(key, context)
        {
            _jellyContext = context as JellyfishStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            _timer = 0f;
            PropelRandomly();
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            if (_jellyContext.JellyfishMovement == null) return;

            _timer += Time.deltaTime;
            if (_timer >= _jellyContext.JellyfishMovement.WaitTimeAfterPropel)
            {
                _timer = 0f;
                PropelRandomly();
            }
        }

        private void PropelRandomly()
        {
            if (_jellyContext.JellyfishMovement != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                _jellyContext.JellyfishMovement.Propel(randomDir);
            }
        }

        public override void CheckSwitchStates()
        {

        }
    }
}