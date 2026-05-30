using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class CharacterStats
    {
        public Stat MoveSpeed { get; }
        public Stat MaxHealth { get; }
        public Stat Defense { get; }

        public CharacterStats(CharacterSO data)
        {
            MoveSpeed = new(data.BaseSpeed);
            MaxHealth = new(data.BaseMaxHealth);
            Defense = new(data.BaseDefense);
        }

        public void TickBuffs(float deltaTime)
        {
            
        }
    }
}