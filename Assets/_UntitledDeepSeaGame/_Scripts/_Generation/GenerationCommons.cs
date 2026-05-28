using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
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
        BackgroundTilemap,
        AirTilemap
    }
}