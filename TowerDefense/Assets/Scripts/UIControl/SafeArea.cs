using System;
using UnityEngine;

namespace UIControl
{
    public class SafeArea : MonoBehaviour
    {
        public static bool activeSafeArea { get; private set; }

        private RectTransform GetTopParent()
        {
            var parent = transform.parent.GetComponent<RectTransform>();
            while (parent != null)
            {
                if (parent.parent == null) return parent;
                parent = parent.parent as RectTransform;
            }

            return null;
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            var safeAreaMin = Screen.safeArea.min;
            if (safeAreaMin.x <= 0) return;
            activeSafeArea = true;
            var rect = GetComponent<RectTransform>();
            var parentRect = GetTopParent();
            var thisRect = parentRect.rect;

            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);

            thisRect.width -= safeAreaMin.x * 2;
            thisRect.height -= Screen.safeArea.position.y;

            rect.sizeDelta = new Vector2(thisRect.width, thisRect.height);
        }
    }
}