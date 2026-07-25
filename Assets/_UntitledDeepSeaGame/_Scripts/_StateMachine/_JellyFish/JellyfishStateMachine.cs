using UnityEngine;

namespace DeepSeaGame
{
    public class JellyfishStateMachine : StateMachine
    {
        

        public JellyfishStateMachine(ServerCharacter character)
        {
            // This constructor gets played on all client machines
            _serverCharacter = character;

            // Sub States
            _states[AIState.Moving] = new JellyfishMovingState(AIState.Moving, this);
            _states[AIState.Pursuing] = new JellyfishPursuingState(AIState.Pursuing, this);
            _states[AIState.Knockbacked] = new JellyfishKnockbackState(AIState.Knockbacked, this);

            // Super States
            _states[AIState.Locomotion] = new JellyfishLocomotionState(AIState.Locomotion, this);
            _states[AIState.Dead] = new JellyfishDeadState(AIState.Dead, this);

            _currentState = _states[AIState.Locomotion];
        }

        public override void ReceiveHP(ServerCharacter inflicter, int amount)
        {
            if (inflicter != null)
            {
                if (amount < 0)
                {
                    
                }
                else
                {
                    // Healed
                }
            }
        }
    }
}
