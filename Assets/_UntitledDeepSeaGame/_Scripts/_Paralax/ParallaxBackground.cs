using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [ExecuteInEditMode]
    public class ParallaxBackground : MonoBehaviour
    {
        public ParallaxCamera ParallaxCamera;
        
        private List<ParallaxLayer> _parallaxLayers = new();

        private void Start()
        {
            if (ParallaxCamera == null)
                ParallaxCamera = Camera.main.GetComponent<ParallaxCamera>();

            if (ParallaxCamera != null)
                ParallaxCamera.OnCameraTranslate += Move;

            SetLayers();
        }

        private void SetLayers()
        {
            _parallaxLayers.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

                if (layer != null)
                {
                    layer.name = "Layer-" + i;
                    _parallaxLayers.Add(layer);
                }
            }
        }

        private void Move(float delta)
        {
            foreach (ParallaxLayer layer in _parallaxLayers)
            {
                layer.Move(delta);
            }
        }
    }
}