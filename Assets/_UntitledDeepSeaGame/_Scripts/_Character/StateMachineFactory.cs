using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class StateMachineFactory
    {
        public static StateMachine CreateStateMachine(ServerCharacter serverCharacter, StateMachineType stateMachineType)
        {
            switch (stateMachineType)
            {
                case StateMachineType.Player:
                    return new PlayerStateMachine(serverCharacter);
                case StateMachineType.Jellyfish:
                    return new JellyfishStateMachine(serverCharacter);
                default:
                    throw new NotSupportedException($"No StateMachine Selected");
            }
        }
    }

    public enum StateMachineType
    {
        Player,
        Jellyfish
    }
}