using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishDeadState : BaseState
    {
        private readonly JellyfishStateMachine _ctx;
        private float _despawnTimer;

        public JellyfishDeadState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true; // This is a super state
            _ctx = Context as JellyfishStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            _despawnTimer = 2f; // Wait 2 seconds for client death animations before despawning
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            if (_despawnTimer > 0)
            {
                _despawnTimer -= Time.deltaTime;
                if (_despawnTimer <= 0)
                {
                    if (NpcManager.Instance != null)
                    {
                        NpcManager.Instance.DespawnNpc(_ctx.ServerCharacter);
                    }
                }
            }
        }

        public override void CheckSwitchStates()
        {

        }

        public override void ClientEnterState(AIStateData stateData)
        {
            Debug.Log($"Entering DeadState");
            // NTFS: Player death animations here, just turn off visuals for now
            _ctx.ServerCharacter.ClientFeedbacks.PlayDeathFeedbacks(stateData.Payload);
            _ctx.ServerCharacter.ClientCharacter.Visuals.SetActive(false);
        }

        public override void ClientExitState(AIStateData stateData)
        {
            _ctx.ServerCharacter.ClientCharacter.Visuals.SetActive(true);
        }
    }
}