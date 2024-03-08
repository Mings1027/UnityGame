using UnityEngine;

namespace UIControl
{
    public class AutoResolution : MonoBehaviour
    {
        [SerializeField] private RectTransform topLetterBox;
        [SerializeField] private RectTransform bottomLetterBox;

        private void Start()
        {
            SetResolution();
            SetLetterBox();
        }

        private void SetResolution()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var rect = cam.rect;

            var scaleHeight = (float)Screen.width / Screen.height * (9f / 16f);

            if (scaleHeight < 1)
            {
                rect.height = scaleHeight;
                rect.y = (1f - scaleHeight) / 2f;
            }

            cam.rect = rect;
        }

        private void SetLetterBox()
        {
            //Anchor Preset Stretch Stretch
            topLetterBox.anchorMin = Vector2.zero;
            topLetterBox.anchorMax = Vector2.one;
            topLetterBox.pivot = Vector2.one * 0.5f;

            //Anchor Preset Stretch Stretch
            bottomLetterBox.anchorMin = Vector2.zero;
            bottomLetterBox.anchorMax = Vector2.one;
            bottomLetterBox.pivot = Vector2.one * 0.5f;

            var uiHeight = bottomLetterBox.rect.height - 1080;
            var letterBoxHeight = uiHeight / 2;

            //Anchor Preset Top Stretch
            topLetterBox.anchorMin = new Vector2(0, 1);
            topLetterBox.anchorMax = Vector2.one;
            topLetterBox.pivot = new Vector2(0.5f, 1);
            topLetterBox.sizeDelta = new Vector2(Screen.width, letterBoxHeight);

            //Anchor Preset Bottom Stretch
            bottomLetterBox.anchorMin = Vector2.zero;
            bottomLetterBox.anchorMax = new Vector2(1, 0);
            bottomLetterBox.pivot = new Vector2(0.5f, 0);
            bottomLetterBox.sizeDelta = new Vector2(Screen.width, letterBoxHeight);

            if (topLetterBox.sizeDelta.y >= 1) return;

            Destroy(topLetterBox.gameObject);
            Destroy(bottomLetterBox.gameObject);
        }
    }
}