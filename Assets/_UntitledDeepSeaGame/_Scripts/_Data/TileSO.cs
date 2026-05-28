using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Tile Data", menuName = "Data/TileData")]
    public class TileSO : RuleTile
    {
        [field: Header("TileData Properties")]
        [Tooltip("Name of the resource world object")]
        public string StringID;
        [field: SerializeField] public WorldTm TileType { get; private set; }
        [field: SerializeField] public ToolType RequiredToolType { get; private set; } = ToolType.Drill;
        [field: SerializeField] public float Hardness { get; private set; } = 0.65f;
        [field: SerializeField, Min(0)] public float LightValue { get; private set; }
        [field: SerializeField] public TileItemSO TileItemSO { get; private set; }
        [field: SerializeField] public List<Loot> ItemDropTable { get; private set; }

        // [field: Header("Game Feel")]
        // [field: SerializeField] public EventReference MiningSound { get; private set; }
        // [field: SerializeField] public EventReference PlaceSound { get; private set; }
        // [field: SerializeField] public EventReference DestroySound { get; private set; }
        // [field: SerializeField] public List<Sprite> MiningParticleSprites { get; private set; }
    }
}
