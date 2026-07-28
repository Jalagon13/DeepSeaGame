using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishPursuingState : BaseState
    {
        private JellyfishStateMachine _jellyContext;
        private float _timer;

        public JellyfishPursuingState(AIState key, StateMachine context) : base(key, context)
        {
            _jellyContext = context as JellyfishStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            _timer = 0f;
            PropelTowardsPlayer();
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
                PropelTowardsPlayer();
            }
        }

        private void PropelTowardsPlayer()
        {
            if (_jellyContext.JellyfishMovement != null && _jellyContext.NearestPlayer != null)
            {
                Vector2 myPos = _jellyContext.ServerCharacter.transform.position;
                Vector2 targetPos = _jellyContext.NearestPlayer.transform.position;
                Vector2 dir = (targetPos - myPos).normalized;
                _jellyContext.JellyfishMovement.Propel(dir);
            }
        }

        public override void CheckSwitchStates()
        {

        }
    }
}