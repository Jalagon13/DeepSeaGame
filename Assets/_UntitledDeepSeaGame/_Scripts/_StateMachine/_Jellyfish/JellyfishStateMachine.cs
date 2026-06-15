using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class JellyfishStateMachine : StateMachine
    {
        public JellyfishStateMachine(ServerCharacter character)
        {
            // This constructor gets played on all client machines
            _serverCharacter = character;

            // Sub States
            _states[AIState.Idle] = new JellyfishIdleState(AIState.Idle, this);
            _states[AIState.Moving] = new JellyfishMoveState(AIState.Moving, this);
            _states[AIState.Knockbacked] = new JellyfishKnockbackState(AIState.Knockbacked, this);

            // Super States
            _states[AIState.Grounded] = new JellyfishGroundedState(AIState.Grounded, this);
            _states[AIState.Dead] = new JellyfishDeadState(AIState.Dead, this);

            _currentState = _states[AIState.Grounded];
        }

        public override void ReceiveHP(ServerCharacter inflicter, int amount)
        {
            if (inflicter != null)
            {
                if (amount < 0)
                {
                    // Damaged
                }
                else
                {
                    // Healed
                }
            }
        }
    }
}