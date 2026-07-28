using UnityEngine;

namespace DeepSeaGame
{
    public class FishStateMachine : StateMachine
    {
        public ServerCharacter LatestAttacker { get; private set; }

        public FishStateMachine(ServerCharacter character)
        {
            // This constructor gets played on all client machines
            _serverCharacter = character;

            // Sub States
            _states[AIState.Moving] = new FishMoveState(AIState.Moving, this);
            _states[AIState.Knockbacked] = new FishKnockbackState(AIState.Knockbacked, this);
            _states[AIState.Fleeing] = new FishFleeingState(AIState.Fleeing, this);

            // Super States
            _states[AIState.Locomotion] = new FishLocomoationState(AIState.Locomotion, this);
            _states[AIState.Dead] = new FishDeadState(AIState.Dead, this);

            _currentState = _states[AIState.Locomotion];
        }

        public override void ReceiveHP(ServerCharacter inflicter, int amount)
        {
            if (inflicter != null)
            {
                if (amount < 0)
                {
                    LatestAttacker = inflicter;
                }
                else
                {
                    // Healed
                }
            }
        }

        public void ClearLatestAttacker(ServerCharacter attacker)
        {
            if (LatestAttacker == attacker)
            {
                LatestAttacker = null;
            }
        }
    }
}
