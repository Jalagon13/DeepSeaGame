using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class KnockbackHandler : MonoBehaviour
    {
        private ServerCharacter _serverCharacter;
    
        public KnockbackHandler(ServerCharacter character)
        {
            _serverCharacter = character;
        }
        
        
    }
}