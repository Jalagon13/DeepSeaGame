using UnityEngine;

namespace DeepSeaGame
{
    public class MultiTileInstance
    {
        public Vector2Int Anchor { get; }
        public TileSO TileSO { get; }
        public MultiTileBehavior Behavior { get; }
        public float Timer { get; set; }

        public MultiTileInstance(Vector2Int anchor, TileSO tileSO, MultiTileBehavior behavior)
        {
            Anchor = anchor;
            TileSO = tileSO;
            Behavior = behavior;
            Timer = 0f;
        }
    }
}
