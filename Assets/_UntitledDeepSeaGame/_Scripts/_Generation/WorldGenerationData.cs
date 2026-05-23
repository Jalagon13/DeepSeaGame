using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class WorldGenerationData : MonoBehaviour
    {
        [Header("World Size")]
        [SerializeField] private int _worldWidth = 1024;
        [SerializeField] private int _worldHeight = 384;

        [Header("Seed")]
        [field: SerializeField]
        public string Seed { get; private set; }
        [SerializeField] private string _defaultSeed = "prototype-world";

        [Header("Execution")]
        [SerializeField] private int _columnsPerFrame = 32;

        public int WorldWidth => _worldWidth;
        public int WorldHeight => _worldHeight;
        public string ResolvedSeed => string.IsNullOrWhiteSpace(Seed) ? _defaultSeed : Seed;
        public int ColumnsPerFrame => Mathf.Max(1, _columnsPerFrame);
    }
}
