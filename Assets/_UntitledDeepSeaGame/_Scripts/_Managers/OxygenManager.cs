using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class OxygenManager : MonoBehaviour
    {
        public static OxygenManager Instance { get; private set; }
        
        [SerializeField] 
        private int _maxOxygen = 100;
        
        private int _currentOxygen;
        
        private void Awake() 
        {
            Instance = this;    
        }
        
        
    }
}
