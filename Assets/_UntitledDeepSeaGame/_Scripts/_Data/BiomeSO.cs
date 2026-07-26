using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSeaGame
{
    [CreateAssetMenu(fileName = "New Biome Data", menuName = "Data/BiomeData")]
    public class BiomeSO : ScriptableObject
    {
        public BiomeType BiomeType;

        [Header("Spawn Parameters")]
        public int MaxNpcSlotAmount = 6;
        [Tooltip("How many NPCs spawn per minute in this biome")]
        [Range(0f, 60f)]
        public float SpawnsPerMinute = 10f;

        [Header("Spawn Pool")]
        public List<BiomeSpawnEntry> SpawnEntries = new();

        // Helper to get a random NPC based on weights
        public CharacterSO GetRandomNpc()
        {
            float totalWeight = 0;
            foreach (var entry in SpawnEntries) totalWeight += entry.SpawnWeight;

            float roll = UnityEngine.Random.Range(0, totalWeight);
            float currentWeight = 0;

            foreach (var entry in SpawnEntries)
            {
                currentWeight += entry.SpawnWeight;
                if (roll <= currentWeight)
                    return entry.Npc;
            }
            return null;
        }
    }

    [Serializable]
    public struct BiomeSpawnEntry
    {
        public CharacterSO Npc;
        public float SpawnWeight; // Higher number = more likely to spawn compared to others
    }

    public enum BiomeType
    {
        None,
        Surface,
        Underground
    }
}