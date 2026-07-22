using System.Collections;
using UnityEngine;

namespace DeepSeaGame
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _loadingScreen;

        private void Awake()
        {
            Show();
        }

        private void Start()
        {
            StartCoroutine(WaitForWorldReadyRoutine());
        }

        private IEnumerator WaitForWorldReadyRoutine()
        {
            // Wait until WorldManager instance exists
            yield return new WaitUntil(() => WorldManager.Instance != null);

            // Wait until the world generation and initialization is completed
            yield return new WaitUntil(() => WorldManager.Instance.IsWorldReady);

            // Hide the loading screen
            Hide();
        }
        
        private void Show()
        {
            _loadingScreen.SetActive(true);
        }

        private void Hide()
        {
            _loadingScreen.SetActive(false);
        }
    }
}
