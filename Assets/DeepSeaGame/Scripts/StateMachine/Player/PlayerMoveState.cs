using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace DeepSeaGame
{
    public class PlayerMoveState : BaseState
    {
        private PlayerStateMachine _ctx;
        private Timer _playSwimSoundTimer;
        private float _swimSoundCooldown = 0.75f;

        public PlayerMoveState(AIState key, StateMachine context) : base(key, context)
        {
            _ctx = Context as PlayerStateMachine;
        }

        protected override void EnterState(AIStateData stateData)
        {
            // Debug.Log("Player switched to move state");
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PlayerSwimSFX, Player.Instance.transform.position);

            _playSwimSoundTimer = new(_swimSoundCooldown);
            _playSwimSoundTimer.OnTimerEnd += PlaySwimSound;
        }

        public override void ExitState()
        {
            // Debug.Log($"Exit move state");
            _playSwimSoundTimer.OnTimerEnd -= PlaySwimSound;
            _playSwimSoundTimer.IsPaused = true;
            _playSwimSoundTimer = null;
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.ServerCharacter.MovementState.Value == MovementState.Idle)
            {
                SwitchState(new AIStateData(AIState.Idle));
            }
            else if (_ctx.ServerCharacter.MovementState.Value == MovementState.Knockback)
            {
                SwitchState(new AIStateData(AIState.Knockbacked));
            }
        }

        public override void UpdateState()
        {
            _playSwimSoundTimer.Tick(Time.deltaTime);
        }

        private void PlaySwimSound(object sender, EventArgs e)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PlayerSwimSFX, Player.Instance.transform.position);
            _playSwimSoundTimer.Reset();
        }
    }
}
