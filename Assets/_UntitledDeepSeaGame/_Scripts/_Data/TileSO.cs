using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "New Tile Data", menuName = "Data/TileData")]
    public class TileSO : RuleTile
    {
        [field: Header("TileSO Properties")]
        [field: SerializeField, Tooltip("Name of the resource world object")]
        public string StringID { get; private set; }
        [field: SerializeField] public WorldTm TileType { get; private set; }
        [field: SerializeField] public ToolType RequiredToolType { get; private set; } = ToolType.Drill;
        [field: SerializeField] public float Hardness { get; private set; } = 0.65f;
        [field: SerializeField, Min(0)] public float LightValue { get; private set; }
        [field: SerializeField] public bool IsSolid { get; private set; } = true;
        [field: SerializeField, Tooltip("If true, acts as a boundary for enclosed spaces (like for the Shelter Core) even if IsSolid is false.")] 
        public bool ActsAsEnclosure { get; private set; } = false;
        [field: SerializeField] public TileItemSO TileItemSO { get; private set; }
        [field: SerializeField] public List<Loot> ItemDropTable { get; private set; }

        [field: Header("MultiTileSO Properties")]
        [field: SerializeField] public bool IsMultiTile { get; private set; } = false;
        [field: SerializeField] public Vector2Int Size { get; private set; } = new Vector2Int(1, 1);
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public MultiTileBehavior Behavior { get; private set; }
        
        // [field: Header("Game Feel")]
        // [field: SerializeField] public EventReference MiningSound { get; private set; }
        // [field: SerializeField] public EventReference PlaceSound { get; private set; }
        // [field: SerializeField] public EventReference DestroySound { get; private set; }
        // [field: SerializeField] public List<Sprite> MiningParticleSprites { get; private set; }
        
        
    }
}
