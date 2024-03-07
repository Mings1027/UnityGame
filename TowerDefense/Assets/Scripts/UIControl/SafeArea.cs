using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private Image minImage;
        [SerializeField] private Image maxImage;

        private void Start()
        {
            var rect = GetComponent<RectTransform>();

            var minAnchor = Screen.safeArea.position;
            var maxAnchor = minAnchor + Screen.safeArea.size;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;
            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;

            rect.anchorMin = minAnchor;
            rect.anchorMax = maxAnchor;

            minImage.rectTransform.anchoredPosition = minAnchor;
            maxImage.rectTransform.anchoredPosition = maxAnchor;
        }
    }
}