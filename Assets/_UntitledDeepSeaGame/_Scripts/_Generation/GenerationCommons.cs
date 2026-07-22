using System;
using UnityEngine;

namespace DeepSeaGame
{
    public enum WorldGenerationState
    {
        NotStarted,
        Initializing,
        GeneratingSurface,
        FillingTerrain,
        CarvingCaves,
        CarvingCaveEntrances,
        PlacingTitaniumOre,
        FinalizingSpawn,
        Completed
    }
    
    public enum WorldTm
    {
        ForegroundTilemap,
        BackgroundTilemap,
        AirTilemap
    }
}