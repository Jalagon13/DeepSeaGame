using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Player Data", menuName = "Data/PlayerData")]
    public class PlayerCharacterSO : CharacterSO
    {
        [Space]
        [Header("Player")]
        [Tooltip("Duration of invincibility frames when character is hit")]
        public float IFrameDuration = 0.17f;
        public int BaseOxygenDuration = 100;
        public float OxygenRefillDuration = 2;
        [Tooltip("When you have no more oxygen, every OxygenDepletedTimeBetweenDamage in seconds, you take OxygenDepletedDamage")]
        public float OxygenDepletedTimeBetweenDamage = 5f;
        public int OxygenDepletedDamage = 10;
    }
}
