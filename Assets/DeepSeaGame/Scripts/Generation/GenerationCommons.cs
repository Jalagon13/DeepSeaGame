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
        PlacingIronOre,
        PlacingKelp,
        FinalizingSpawn,
        Completed
    }
    
    public enum WorldTm
    {
        ForegroundTilemap,
        BackgroundTilemap,
        AirTilemap
    }

    public enum TileBreakMode
    {
        SingleTileHit,
        FromHitTileUp,
        FromHitTileDown,
    }
}