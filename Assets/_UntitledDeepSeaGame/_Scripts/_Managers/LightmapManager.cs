using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class LightmapManager : MonoBehaviour
    {
        public static LightmapManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        
    }
}
