using Sirenix.OdinInspector;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class DamageTest : MonoBehaviour
    {
        private DamageReceiver _damageReceiver;
        private ServerCharacter inflicter;
        
        private void Awake() 
        {
            _damageReceiver = GetComponent<DamageReceiver>();
            inflicter = GetComponent<ServerCharacter>();
        }
        
        [Button("TestDamage")]
        public void TestDamage(int amount)
        {
            _damageReceiver.ReceiveHP(inflicter, -amount, true);
        }
    }
}