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
}