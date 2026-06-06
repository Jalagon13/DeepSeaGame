using UnityEngine;

namespace UntitledDeepSeaGame
{
    [ExecuteInEditMode]
    public class ParallaxLayer : MonoBehaviour
    {
        public float ParallaxFactor;
        
        private WorldGenerationData _worldGenerationData;
        private SpriteRenderer _spriteRenderer;
        private bool _hasBeenInitalized;
        
        private void Awake() 
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(WorldGenerationData worldGenerationData)
        {
            _worldGenerationData = worldGenerationData;
            
            int worldWidth = _worldGenerationData.WorldWidth;
            int undergroundZoneHeight = _worldGenerationData.UndergroundMaxYLevel - _worldGenerationData.UndergroundMinYLevel;

            _spriteRenderer.size = new Vector2(worldWidth/*  * 0.5f */, undergroundZoneHeight);
            transform.position = new Vector3(worldWidth * 0.5f, undergroundZoneHeight * 0.5f);
            _hasBeenInitalized = true;
        }

        public void Move(float delta)
        {
            if(!_hasBeenInitalized) return;
        
            Vector3 newPos = transform.localPosition;
            newPos.x -= delta * ParallaxFactor;

            transform.localPosition = newPos;
        }

    }
}