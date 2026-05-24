using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class FillSandBackgroundStep : GenerationStep
    {
        [Header("Terrain")]
        [SerializeField] private TileSO _sandWallTileSO;

        public override WorldGenerationState State => throw new System.NotImplementedException();

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
