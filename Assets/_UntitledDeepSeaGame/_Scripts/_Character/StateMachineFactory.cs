using System;
using UnityEngine;

namespace DeepSeaGame
{
    public class StateMachineFactory
    {
        public static StateMachine CreateStateMachine(ServerCharacter serverCharacter, StateMachineType stateMachineType)
        {
            switch (stateMachineType)
            {
                case StateMachineType.Player:
                    return new PlayerStateMachine(serverCharacter);
                case StateMachineType.Fish:
                    return new FishStateMachine(serverCharacter);
                default:
                    throw new NotSupportedException($"No StateMachine Selected");
            }
        }
    }

    public enum StateMachineType
    {
        Player,
        Fish
    }
}