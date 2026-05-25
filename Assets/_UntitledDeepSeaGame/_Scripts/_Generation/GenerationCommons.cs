using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public enum TileType
    {
        ForegroundTile,
        BackgroundTile
    }

    public enum WorldGenerationState
    {
        NotStarted,
        Initializing,
        GeneratingSurface,
        FillingTerrain,
        CarvingCaves,
        FinalizingSpawn,
        Completed
    }
    
    public enum WorldTm
    {
        ForegroundTilemap,
        BackgroundTilemap
    }

    [Serializable]
    public class BiomeBackgroundLayer
    {
        [Tooltip("Name of the biome layer (e.g., Underground, Cave, Space)")]
        public string layerName;

        [Tooltip("The repeating background texture sprite. Note: Sprite wrap mode must be set to Repeat in import settings.")]
        public Sprite backgroundSprite;

        [Tooltip("Lower bounds of this layer in world space coordinates (Y coordinate)")]
        public float minY;

        [Tooltip("Upper bounds of this layer in world space coordinates (Y coordinate)")]
        public float maxY;

        [Range(0f, 1f), Tooltip("Horizontal parallax scrolling speed factor (0 = moves fully with camera, 1 = locked to world)")]
        public float parallaxFactorX = 0.5f;

        [Tooltip("Sorting order for the background SpriteRenderer (should be lower than tilemaps, e.g. -100)")]
        public int sortingOrder = -100;

        [HideInInspector]
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public GameObject layerGameObject;
    }
}