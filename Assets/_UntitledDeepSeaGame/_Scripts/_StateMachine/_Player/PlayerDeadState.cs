using UnityEngine;

namespace DeepSeaGame
{
    public class PlayerDeadState : BaseState
    {
        private PlayerStateMachine _ctx;

        public PlayerDeadState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true;
            _ctx = Context as PlayerStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            Debug.Log("Player entering dead");
            _ctx.ServerCharacter.ClientCharacter.ColliderHolder.SetActive(false);
        }

        public override void UpdateState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (_ctx.ServerCharacter.LifeState == LifeState.IFrame)
            {
                SwitchState(new AIStateData(AIState.Locomotion));
            }
        }

        public override void ExitState()
        {
            if (_ctx.ServerCharacter.TryGetComponent(out Collider2D collider2D))
            {
                collider2D.enabled = true;
            }

            _ctx.ServerCharacter.ClientCharacter.ColliderHolder.gameObject.SetActive(true);
        }

        public override void ClientEnterState(AIStateData stateData)
        {
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