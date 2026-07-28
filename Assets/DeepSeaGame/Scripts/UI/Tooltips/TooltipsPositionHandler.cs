using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepSeaGame
{
    public class TooltipsPositionHandler : MonoBehaviour
    {
        public RectTransform Canvas;
        
        [Tooltip("should be the same one as in the TooltipReferenceHolder")] 
        public RectTransform Layout;
        
        private void Update()
        {
            // moves to clamped position of mouse  
            Vector2 anchorPoint = Mouse.current.position.ReadValue() / Canvas.localScale.x;

            if (anchorPoint.x + Layout.rect.width > Canvas.rect.width)
                anchorPoint.x = Canvas.rect.width - Layout.rect.width;

            if (anchorPoint.y + Layout.rect.height > Canvas.rect.height)
                anchorPoint.y = Canvas.rect.height - Layout.rect.height;


            Layout.anchoredPosition = anchorPoint;
        }
    }
}
