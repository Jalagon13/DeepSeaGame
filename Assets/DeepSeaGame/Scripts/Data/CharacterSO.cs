using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Character Data", menuName = "Data/CharacterData")]
    public class CharacterSO : ScriptableObject
    {
        [Header("Core Stats")]
        [Tooltip("Base Speed for character")]
        public int BaseSpeed;
        public int BaseMaxHealth;
        public int BaseDefense;
        public int Damage = 1;
        public float KnockbackForce = 6f;
        public bool PlayKnockback = true;

        [Space]
        [Header("Sounds")]
        public EventReference DamageSFX;
        public EventReference DeathSFX;

        [Space]
        [Header("Health & Survival")]
        [Tooltip("If true, character can die")]
        public bool CanDie = true;
        
        [Tooltip("If true, the NPC can be knocked back")]
        public bool CanBeKnockedBack = true;

        [Tooltip("Resistance to knockback effects (0 = no resistance, 1 = full resistance)")]
        [Range(0f, 1f)]
        public float KnockbackResist = 0f;
        
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

        [Tooltip("Minimum time the NPC will stay idle before changing state")]
        public float MinIdleDuration = 2.5f;
        
        [Tooltip("Maximum time the NPC will stay idle before changing state")]
        public float MaxIdleDuration = 5f;

        [Tooltip("Loot entries to spawn when this character dies")]
        public List<Loot> LootDrops = new();
    }
} 
