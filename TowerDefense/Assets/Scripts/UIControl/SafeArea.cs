using UnityEngine;

namespace UIControl
{
    public class SafeArea : MonoBehaviour
    {
        private void Start()
        {
            var rect = GetComponent<RectTransform>();

            var minAnchor = Screen.safeArea.min;
            var maxAnchor = Screen.safeArea.max;

            minAnchor.x /= Screen.width;
            minAnchor.y /= Screen.height;

            maxAnchor.x /= Screen.width;
            maxAnchor.y /= Screen.height;
            Debug.Log(minAnchor);
            Debug.Log(maxAnchor);
            rect.anchorMin = minAnchor;
            rect.anchorMax = maxAnchor;
        }
    }
}