using System.Collections;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class PlaceTitaniumOreStep : GenerationStep
    {
        [Header("Ore")]
        [SerializeField] private TileSO _titaniumOreTileSO;

        public override WorldGenerationState State => WorldGenerationState.PlacingTitaniumOre;

        public override IEnumerator Execute(WorldGenerationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
