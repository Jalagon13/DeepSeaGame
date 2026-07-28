using UnityEngine;
using System.Collections;


namespace DeepSeaGame
{
    public abstract class GenerationStep : MonoBehaviour
    {
        [field: SerializeField] public bool ExecuteStep { get; private set; } = true;
        [field: SerializeField] public string Description { get; private set; }
        public abstract WorldGenerationState State { get; }

        public abstract IEnumerator Execute(WorldGenerationContext context);
    }

}
