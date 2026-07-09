using System;
using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerMiningState : BaseState
    {
        private PlayerStateMachine _ctx;

        public PlayerMiningState(AIState key, StateMachine context) : base(key, context)
        {
            IsSuperState = true;
            _ctx = Context as PlayerStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log($"Player enter mining state");
            
            ushort itemId = GameDataRegistry.Instance.GetItemIdFromItemSO(_ctx.HeldItem);
            Player.Instance.PlayerArmController.StartAimHandRpc(itemId);
        }

        public override void UpdateState()
        {
            
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Grounded));
            }
            else if((!GameInput.Instance.PrimaryActionHeldDown && !GameInput.Instance.SecondaryActionHeldDown) || _ctx.HeldItem is not ToolItemSO || (_ctx.HeldItem is ToolItemSO tool && tool.HarvestType != ToolType.Drill))
            {
                SwitchState(new AIStateData(AIState.Grounded));
            }
        }

        public override void ExitState()
        {
            Player.Instance.PlayerArmController.EndAimHandRpc();
        }

        

        
    }
}
