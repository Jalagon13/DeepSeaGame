using FMODUnity;
using UnityEngine;

namespace DeepSeaGame
{
    public class FMODEvents : MonoBehaviour
    {
        public static FMODEvents Instance { get; private set; }

        [field: Header("Ambience")]
        [field: SerializeField] public EventReference OceanAmbience { get; private set; }
        [field: SerializeField] public EventReference TitleMusic { get; private set; }

        private void Awake() 
        {
            Instance = this;    
        }
        
        
    }
}
