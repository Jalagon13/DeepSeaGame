using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class MultiTileInstance
    {
        public Vector2Int Anchor { get; }
        public TileSO TileSO { get; }
        public MultiTileLifecycleBehavior Behavior { get; }
        public float Timer { get; set; }

        public MultiTileInstance(Vector2Int anchor, TileSO tileSO, MultiTileLifecycleBehavior behavior)
        {
            Anchor = anchor;
            TileSO = tileSO;
            Behavior = behavior;
            Timer = 0f;
        }
    }
}
