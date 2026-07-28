using System;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSeaGame
{
    public class RadioUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _radioScreen;
        [SerializeField] private Button _closeButton;
    
        private void Awake() 
        {
            Hide();
            
            _closeButton.onClick.AddListener(() => 
            {
               Hide(); 
            });
        }
    
        private void Start() 
        {
            GameManager.Instance.OnPrototypeEnd += OnPrototypeEnd;
        }
        
        private void OnDestroy() 
        {
            GameManager.Instance.OnPrototypeEnd -= OnPrototypeEnd;
        }

        private void OnPrototypeEnd()
        {
            Show();
        }
        
        private void Show()
        {
            _radioScreen.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        
        private void Hide()
        {
            _radioScreen.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
