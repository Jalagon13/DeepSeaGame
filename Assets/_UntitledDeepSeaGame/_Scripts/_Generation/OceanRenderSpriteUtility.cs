using UnityEngine;

namespace UntitledDeepSeaGame
{
    internal static class OceanRenderSpriteUtility
    {
        private static Sprite s_unitSprite;

        public static Sprite UnitSprite
        {
            get
            {
                if (s_unitSprite != null)
                {
                    return s_unitSprite;
                }

                Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                s_unitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                s_unitSprite.hideFlags = HideFlags.HideAndDontSave;
                return s_unitSprite;
            }
        }
    }
}
