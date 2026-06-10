using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerIdleState : BaseState
    {

        public PlayerIdleState(AIState key, StateMachine context) : base(key, context)
        {
            
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log("Player switched to idle state");
        }

        public override void ExitState()
        {

        }

        public override void CheckSwitchStates()
        {
            if (Context.ServerCharacter.MovementState.Value == MovementState.Moving)
            {
                SwitchState(new AIStateData(AIState.Moving));
            }
            else if (Context.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked));
            }
        }

        public override void UpdateState()
        {

        }
    }
}
