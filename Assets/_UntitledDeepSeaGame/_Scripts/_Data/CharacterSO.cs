using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Character Data", menuName = "Data/CharacterData")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Core Stats")]
        [Tooltip("Base Speed for character")]
        public int BaseSpeed;
        public int BaseMaxHealth;
        public int BaseDefense;

        [Space]
        [Header("Health & Survival")]
        [Tooltip("If true, character can die")]
        public bool CanDie = true;
        [Tooltip("If true, the NPC can be knocked back")]
        public bool CanBeKnockedBack = true;
        [Tooltip("Duration of invincibility frames when character is hit")]
        public float IFrameDuration = 0.17f;


        [Space]
        [Header("Movement & Physics")]
        [Tooltip("If false, the NPC will remain idle and not move")]
        public bool CanMove = true;
        [Tooltip("Smaller values = slower transition to desired direction")]
        public int TurnSharpness = 5;
        

        [Space]
        [Header("NPC Parameters")]
        [Tooltip("Indicates whether the character is an NPC")]
        public bool IsNpc;
        [Tooltip("The amount of 'npc space' the NPC take up when spawned")]
        public float SlotAmount;
        [Tooltip("Prefab for the NPC")]
        public ServerCharacter NpcPrefab;
    }
}
