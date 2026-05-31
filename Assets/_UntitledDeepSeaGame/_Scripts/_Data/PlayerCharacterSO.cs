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
        public int BaseMaxOxygen = 100;
    }
}
