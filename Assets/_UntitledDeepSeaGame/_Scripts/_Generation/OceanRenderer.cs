using UnityEngine;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OceanRenderer : MonoBehaviour
    {
        [SerializeField] private WorldGenerationData _worldGenerationData;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        public void Initialize(WorldGenerationData worldGenerationData)
        {
            _worldGenerationData = worldGenerationData;
            Refresh();
        }

        public void Refresh()
        {
            if(_worldGenerationData == null)
            {
                return; 
            }
        
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            float width = _worldGenerationData.WorldWidth;
            float height = _worldGenerationData.SeaLevelY;

            _spriteRenderer.sprite = OceanRenderSpriteUtility.UnitSprite;

            transform.position = new Vector3(width * 0.5f, height * 0.5f);
            transform.localScale = new Vector3(width, height, 1f);
        }
    }
}
