using UnityEngine;

namespace UntitledDeepSeaGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class OceanRenderer : MonoBehaviour
    {
        [SerializeField] private WorldGenerationData _worldGenerationData;
        [SerializeField] private Color _waterColor = new(0.05f, 0.42f, 0.72f, 0.5f);
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
            float height = generationData.SeaLevelY;

            _spriteRenderer.sprite = OceanRenderSpriteUtility.UnitSprite;
            _spriteRenderer.color = _waterColor;
            _spriteRenderer.sortingOrder = _sortingOrder;

            transform.position = new Vector3(width * 0.5f, height * 0.5f, _zPosition);
            transform.localScale = new Vector3(width, height, 1f);
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
