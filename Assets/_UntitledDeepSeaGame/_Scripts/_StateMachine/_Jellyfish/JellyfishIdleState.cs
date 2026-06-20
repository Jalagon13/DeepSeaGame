using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class JellyfishIdleState : BaseState
    {
        private Timer _idleTimer;
        private bool _idleComplete;
        private JellyfishStateMachine _ctx;
        private JellyfishCharacterMovement _jellyfishMovement;


        public JellyfishIdleState(AIState key, StateMachine context) : base(key, context)
        {
            _ctx = Context as JellyfishStateMachine;
            
            _jellyfishMovement = _ctx.ServerCharacter.Movement as JellyfishCharacterMovement;
            if(_jellyfishMovement == null)
            {
                Debug.LogError($"Jellyfish movement script could not be found.");
            }
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log($"Jellyfish Idle State Entered");
            _idleComplete = false;

            float idleDuration = Random.Range(_ctx.ServerCharacter.CharacterData.MinIdleDuration, _ctx.ServerCharacter.CharacterData.MaxIdleDuration);
            if (idleDuration <= 0)
            {
                idleDuration = 0.0001f;
            }

            _idleTimer = new(idleDuration);
            _idleTimer.OnTimerEnd += OnIdleDone;

            _jellyfishMovement.StartIdle();
        }

        private void OnIdleDone(object sender, System.EventArgs e)
        {
            _idleTimer.OnTimerEnd -= OnIdleDone;
            _idleComplete = true;
        }

        public override void ExitState()
        {
            _idleTimer.OnTimerEnd -= OnIdleDone;
        }

        public override void UpdateState()
        {
            _idleTimer.Tick(Time.deltaTime);
        }

        public override void CheckSwitchStates()
        {
            if (_idleComplete && _ctx.ServerCharacter.CharacterData.CanMove && _ctx.ServerCharacter.CharacterData.BaseSpeed != 0)
            {
                SwitchState(new AIStateData(AIState.Moving));
            }
            else if (_ctx.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked));
            }
        }
    }
}