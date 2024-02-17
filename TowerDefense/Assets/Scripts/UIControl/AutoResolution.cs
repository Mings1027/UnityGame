using UnityEngine;

namespace UIControl
{
    public class AutoResolution : MonoBehaviour
    {
        private float _uiHeight;

        [SerializeField] private RectTransform topLetterBox;
        [SerializeField] private RectTransform bottomLetterBox;

        private void Start()
        {
            Debug.Log(Screen.width);
            Debug.Log(Screen.height);
            SetResolution();
            SetLetterBox();
        }

        private void SetResolution()
        {
            var cam = Camera.main;
            var rect = cam.rect;

            var scaleHeight = (float)Screen.width / Screen.height * (9f / 16f);
            // var scaleWidth = 1f / scaleHeight;

            if (scaleHeight < 1)
            {
                Debug.Log("1보다 작");
                rect.height = scaleHeight;
                rect.y = (1f - scaleHeight) / 2f;
            }
            else
            {
                Debug.Log("1보다 큼");
                // rect.width = scaleWidth;
                // rect.x = (1f - scaleWidth) / 2f;
            }

            cam.rect = rect;
        }

        private void SetLetterBox()
        {
            _uiHeight = bottomLetterBox.rect.height - 1080;
            var letterBoxHeight = _uiHeight / 2;

            topLetterBox.anchorMin = new Vector2(0, 1);
            topLetterBox.anchorMax = new Vector2(1, 1);
            topLetterBox.pivot = new Vector2(0.5f, 1);
            topLetterBox.sizeDelta = new Vector2(Screen.width, letterBoxHeight);

            bottomLetterBox.anchorMin = Vector2.zero;
            bottomLetterBox.anchorMax = new Vector2(1, 0);
            bottomLetterBox.pivot = new Vector2(0.5f, 0);
            bottomLetterBox.sizeDelta = new Vector2(Screen.width, letterBoxHeight);

            if (topLetterBox.sizeDelta.y < 1)
            {
                Destroy(topLetterBox.gameObject);
                Destroy(bottomLetterBox.gameObject);
            }
        }
    }
}