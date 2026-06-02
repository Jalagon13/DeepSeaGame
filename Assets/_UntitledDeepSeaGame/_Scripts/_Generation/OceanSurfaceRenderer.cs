using UnityEngine;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OceanSurfaceRenderer : MonoBehaviour
    {
        [SerializeField] private WorldGenerationData _worldGenerationData;
        [SerializeField, Min(0.01f)] private float _surfaceHeight = 0.35f;
        [SerializeField] private Color _surfaceColor = new(0.55f, 0.9f, 1f, 0.7f);
        [SerializeField] private int _sortingOrder = 0;
        [SerializeField] private float _zPosition;

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
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            WorldGenerationData generationData = ResolveGenerationData();
            if (generationData == null)
            {
                return;
            }

            float width = generationData.WorldWidth;
            float seaLevelY = generationData.SeaLevelY;

            _spriteRenderer.sprite = OceanRenderSpriteUtility.UnitSprite;
            _spriteRenderer.color = _surfaceColor;
            _spriteRenderer.sortingOrder = _sortingOrder;

            transform.position = new Vector3(width * 0.5f, seaLevelY, _zPosition);
            transform.localScale = new Vector3(width, _surfaceHeight, 1f);
        }

        private WorldGenerationData ResolveGenerationData()
        {
            if (_worldGenerationData != null)
            {
                return _worldGenerationData;
            }

            _worldGenerationData = GetComponentInParent<WorldGenerationData>();
            if (_worldGenerationData != null)
            {
                return _worldGenerationData;
            }

            _worldGenerationData = FindAnyObjectByType<WorldGenerationData>();
            return _worldGenerationData;
        }
    }
}
