using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    public class FlashlightController : MonoBehaviour
    {
        private void Start()
        {
            GameInput.Instance.OnToggleFlashlight += GameInput_OnToggleFlashlight;
        }
        
        private void OnDestroy()
        {
            GameInput.Instance.OnToggleFlashlight -= GameInput_OnToggleFlashlight;
        }

        private void GameInput_OnToggleFlashlight(object sender, InputAction.CallbackContext e)
        {
            if (e.started)
            {
                ToggleFlashlight();
            }
        }

        private void ToggleFlashlight()
        {
            throw new NotImplementedException();
        }
    }
}
