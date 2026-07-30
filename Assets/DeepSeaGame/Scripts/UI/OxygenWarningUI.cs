using System;
using Unity.Netcode;
using UnityEngine;

namespace DeepSeaGame
{
    public class OxygenWarningUI : NetworkBehaviour
    {
        private void Awake() 
        {
            Player.OnAnyPlayerSpawned += RegisterOxygenWarning;
        }

        public override void OnDestroy()
        {
            Player.OnAnyPlayerSpawned -= RegisterOxygenWarning;
            
            if(Player.Instance != null)
            {
                Player.Instance.PlayerOxygenController.OnOxygenWarning -= OnOxygenWarning;
            }
        }

        private void RegisterOxygenWarning(object sender, Player.PlayerIdEventArgs e)
        {
            if (NetworkManager.LocalClientId != e.PlayerId) return;

            Player.Instance.PlayerOxygenController.OnOxygenWarning += OnOxygenWarning;
        }

        private void OnOxygenWarning()
        {
            Debug.Log($"Oxygen Warning");
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.OxygenWarningSFX, default);
            
            
        }
    }
}
