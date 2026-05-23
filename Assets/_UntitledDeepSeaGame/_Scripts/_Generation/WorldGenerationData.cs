using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldGenerationData : MonoBehaviour
    {
        [SerializeField] 
        private int _worldWidth, _worldHeight;

        public int WorldWidth => _worldWidth;
        public int WorldHeight => _worldHeight;

        [field: SerializeField]
        public string Seed { get; private set; }
    }
}
