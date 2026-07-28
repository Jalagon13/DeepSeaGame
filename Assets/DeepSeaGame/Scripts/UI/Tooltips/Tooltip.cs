using TMPro;
using UnityEngine;

namespace DeepSeaGame
{
    public static class Tooltip
    {
        public static TooltipsInstantiateHandler InstantiateHandler;
        public static TooltipReferenceHolder ReferenceHolder;

        public static void HideUI()
        {
            ReferenceHolder.HideUI();
        }
        public static void ShowUI()
        {
            ReferenceHolder.ShowUI();
        }
        public static void ShowNew()
        {
            ClearOldPrefabs();
            ShowUI();
            ReturnBackgroundToDefault();
            ReferenceHolder.Layout.padding = ReferenceHolder.DefaultPadding;
        }
        
        public static void ReturnBackgroundToDefault()
        {
            ReferenceHolder.Background.sprite = ReferenceHolder.DefaultBackgroundSprite;
            ReferenceHolder.Background.color = ReferenceHolder.DefaultBackgroundColor;

        }
        
        public static void ClearOldPrefabs()
        {
            ReferenceHolder.ClearOldPrefabs();
        }
        
        public static void CustomizeBackground(Sprite sprite, Color color)
        {
            ReferenceHolder.Background.sprite = sprite;
            ReferenceHolder.Background.color = color;
        }

        #region  just text
        
        //  If font == null -> will use default font
        public static void JustText(Sprite icon, Color colorOfIcon, string text, Color colorOfTheText, Transform customLayout = null, TMP_FontAsset font = null, float fontSize = 20)
        {
            JustTextHandler script = InstantiateHandler.InstantiateJustText(customLayout);
            script.icon.sprite = icon;
            script.icon.color = colorOfIcon;

            script.text.font = font == null ? ReferenceHolder.DefaultFont : font;
            script.text.text = text;
            script.text.color = colorOfTheText;
            script.text.fontSize = fontSize;
        }

        public static void JustText(Sprite icon, Color colorOfIcon, string text, Color colorOfTheText, float iconScale, Transform customLayout = null, TMP_FontAsset font = null, float fontSize = 20)
        {
            JustTextHandler script = InstantiateHandler.InstantiateJustText(customLayout);
            script.icon.sprite = icon;
            script.icon.color = colorOfIcon;

            script.text.font = font == null ? ReferenceHolder.DefaultFont : font;
            script.text.text = text;
            script.text.color = colorOfTheText;
            script.text.fontSize = fontSize;
            script.icon.transform.localScale = Vector3.one * iconScale;
        }

        public static void JustText(string text, Color colorOfTheText, TMP_FontAsset font = null, float fontSize = 20, Transform customLayout = null)
        {
            JustText(icon: null, new(0, 0, 0, 0), text, colorOfTheText, font: font, fontSize: fontSize, customLayout: customLayout);
        }
        
        #endregion
    }
}