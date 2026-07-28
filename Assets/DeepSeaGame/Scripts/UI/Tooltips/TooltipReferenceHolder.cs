using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSeaGame
{
    [RequireComponent(typeof(TooltipsInstantiateHandler)), RequireComponent(typeof(TooltipsPositionHandler))]
    public class TooltipReferenceHolder : MonoBehaviour
    {
        public VerticalLayoutGroup Layout;
        [HideInInspector] public List<GameObject> oldPrefabs = new();

        public TMP_FontAsset DefaultFont;

        [Header("Tooltip Prefabs")]
        public GameObject JustTextPrefab;

        [Header("Tooltip Background")]
        public Image Background;
        public Sprite DefaultBackgroundSprite;
        public Color DefaultBackgroundColor;
        public RectOffset DefaultPadding;

        [Header("Tooltip")]
        [SerializeField] private float _tooltipDelay = .3f;

        private bool _turnOn = true;


        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Tooltip.ReferenceHolder = this;
        }

        public void ShowUI()
        {
            Invoke(nameof(TurnOn), _tooltipDelay);
            _turnOn = true;
        }
        
        public void HideUI()
        {
            gameObject.SetActive(false);
            _turnOn = false;
        }

        public void ClearOldPrefabs()
        {
            for (int i = 0; i < oldPrefabs.Count; i++)
            {
                Destroy(oldPrefabs[i]);
            }
            oldPrefabs.Clear();
        }

        // its used to delay showing up of tooltip
        private void TurnOn()
        {
            if (!_turnOn)
                return;

            gameObject.SetActive(true);

        }
        
    }
}
