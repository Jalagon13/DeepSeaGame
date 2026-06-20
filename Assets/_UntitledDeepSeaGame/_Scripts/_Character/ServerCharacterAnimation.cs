using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class ServerCharacterAnimation : NetworkBehaviour
    {
        [SerializeField]
        private ServerCharacter _serverCharacter;

        [SerializeField]
        private NetworkHealthState _networkHealthState;
        [SerializeField]
        private List<ServerSpriteAnimHandler> _spriteAnimHandlers = new List<ServerSpriteAnimHandler>();

        private Direction _actionDirection = Direction.None; // Used for casting direction and swing direction

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _networkHealthState.LifeState.OnValueChanged += OnLifeStateChanged;
                _serverCharacter.MovementState.OnValueChanged += PlayCurrentMoveState;
                _serverCharacter.CurrentDirection.OnValueChanged += OnCardinalDirectionChanged;
                // if (_serverCharacter.TryGetComponent(out Player player))
                // {
                //     player.PlayerHand.SwingDirection.OnValueChanged += OnActionDirectionChanged;
                //     player.PlayerHand.CastingDirection.OnValueChanged += OnActionDirectionChanged;
                // }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && _networkHealthState != null)
            {
                _networkHealthState.LifeState.OnValueChanged -= OnLifeStateChanged;
                _serverCharacter.MovementState.OnValueChanged -= PlayCurrentMoveState;
                _serverCharacter.CurrentDirection.OnValueChanged -= OnCardinalDirectionChanged;
                // if (_serverCharacter.TryGetComponent(out Player player))
                // {
                //     player.PlayerHand.SwingDirection.OnValueChanged -= OnActionDirectionChanged;
                //     player.PlayerHand.CastingDirection.OnValueChanged -= OnActionDirectionChanged;
                // }
            }
        }

        private void OnActionDirectionChanged(Direction previousValue, Direction newValue)
        {
            _actionDirection = newValue;

            foreach (ServerSpriteAnimHandler handler in _spriteAnimHandlers)
            {
                handler.PlayAnimation(_serverCharacter.MovementState.Value, _actionDirection == Direction.None ? _serverCharacter.CurrentDirection.Value : _actionDirection);
            }
        }

        private void OnCardinalDirectionChanged(Direction previousValue, Direction newValue)
        {
            foreach (ServerSpriteAnimHandler handler in _spriteAnimHandlers)
            {
                handler.PlayAnimation(_serverCharacter.MovementState.Value, _actionDirection == Direction.None ? newValue : _actionDirection);
            }
        }

        private void PlayCurrentMoveState(MovementState previousMovementState, MovementState newMovementState)
        {
            Direction direction = _serverCharacter.CurrentDirection.Value;

            foreach (ServerSpriteAnimHandler handler in _spriteAnimHandlers)
            {
                handler.PlayAnimation(newMovementState, _actionDirection == Direction.None ? direction : _actionDirection);
            }
        }

        private void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
        {
            // TODO: Later
            switch (newValue)
            {
                case LifeState.Alive:

                    break;
                case LifeState.IFrame:

                    break;
                case LifeState.Dead:

                    break;
            }
        }
    }
}
