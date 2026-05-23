using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlaceSandGenStep : GenerationStep
    {
        public override WorldGenerationState State => WorldGenerationState.FillingTerrain;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            yield break;
        }
    }
}
