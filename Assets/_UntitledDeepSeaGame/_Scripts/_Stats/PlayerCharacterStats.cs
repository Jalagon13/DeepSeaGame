using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlayerCharacterStats : CharacterStats
    {
        public Stat MaxOxygen { get; }

        public PlayerCharacterStats(PlayerCharacterSO data) : base(data)
        {
            MaxOxygen = new(data.BaseMaxOxygen);
        }
    }
}