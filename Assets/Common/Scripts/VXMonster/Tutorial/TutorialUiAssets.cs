using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Tutorial
{
    [CreateAssetMenu(menuName = "VX Monster/Tutorial UI Assets")]
    public class TutorialUiAssets : ScriptableObject
    {
        public TMP_FontAsset gameFont;
        public TMP_FontAsset plainFont;
        public Sprite windowSprite;
        public Sprite confirmButtonSprite;
        public Sprite skipButtonSprite;
        public Sprite dimSprite;

        public static TutorialUiAssets Instance
        {
            get
            {
                if (cached == null)
                {
                    cached = Resources.Load<TutorialUiAssets>("VXMonster/TutorialUiAssets");
                }

                return cached;
            }
        }

        public static TMP_FontAsset GameFont => Instance != null ? Instance.gameFont : null;
        public static TMP_FontAsset PlainFont => Instance != null && Instance.plainFont != null
            ? Instance.plainFont
            : GameFont;

        public static Sprite WindowSprite => Instance != null ? Instance.windowSprite : null;
        public static Sprite ConfirmButtonSprite => Instance != null ? Instance.confirmButtonSprite : null;
        public static Sprite SkipButtonSprite => Instance != null ? Instance.skipButtonSprite : null;
        public static Sprite DimSprite => Instance != null && Instance.dimSprite != null
            ? Instance.dimSprite
            : WindowSprite;

        public static void StyleDimmer(Image image)
        {
            if (image == null) return;
            image.sprite = DimSprite;
            image.type = Image.Type.Simple;
            image.color = new Color(0.07f, 0.05f, 0.12f, 0.94f);
            image.raycastTarget = true;
        }

        public static void StyleWindow(Image image)
        {
            if (image == null) return;
            image.sprite = WindowSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = 2f;
        }

        public static void StyleConfirmButton(Image image)
        {
            if (image == null) return;
            image.sprite = ConfirmButtonSprite != null ? ConfirmButtonSprite : WindowSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = 1.85f;
        }

        public static void StyleSkipButton(Image image)
        {
            if (image == null) return;
            image.sprite = SkipButtonSprite != null ? SkipButtonSprite : WindowSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.pixelsPerUnitMultiplier = 1.85f;
        }

        static TutorialUiAssets cached;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache()
        {
            cached = null;
        }
    }
}
