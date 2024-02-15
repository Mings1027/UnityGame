using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utilities
{
    public class UIVerticalSplitter : MonoBehaviour
    {
        public float[] ratios; // 각 자식 요소의 비율

        private void Start()
        {
            VerticalSplitter();
        }

        [ContextMenu("Vertical Splitter")]
        public void VerticalSplitter()
        {
            if (ratios.Length <= 0) return;
Debug.Log("verti splitter");
            var totalRatio = ratios.Sum();
            var parentRect = GetComponent<RectTransform>();
            var parentHeight = parentRect.rect.height;
            var childRects = new List<RectTransform>();
            for (var i = 0; i < transform.childCount; i++)
            {
                childRects.Add(transform.GetChild(i).GetComponent<RectTransform>());
            }

            for (var i = 0; i < childRects.Count; i++)
            {
                var childHeight = ratios[i] / totalRatio * parentHeight;
                childRects[i].sizeDelta = new Vector2(parentRect.rect.width, childHeight);
            }
        }
    }
}