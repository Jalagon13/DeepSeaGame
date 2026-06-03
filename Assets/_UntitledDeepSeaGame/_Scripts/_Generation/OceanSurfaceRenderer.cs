using UnityEngine;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OceanSurfaceRenderer : MonoBehaviour
    {
        [SerializeField] private WorldGenerationData _worldGenerationData;
        [SerializeField, Min(0.01f)] private float _surfaceHeight = 0.35f;
        
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
            if (_worldGenerationData == null)
            {
                return;
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            float width = _worldGenerationData.WorldWidth;
            float seaLevelY = _worldGenerationData.SeaLevelY;

            _spriteRenderer.sprite = OceanRenderSpriteUtility.UnitSprite;

            transform.position = new Vector3(width * 0.5f, seaLevelY);
            transform.localScale = new Vector3(width, _surfaceHeight, 1f);
        }
    }
}
