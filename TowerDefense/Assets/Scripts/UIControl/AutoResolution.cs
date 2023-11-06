using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class AutoResolution : MonoBehaviour
    {
        private CanvasScaler _canvas;

        private void Start()
        {
            _canvas = GetComponent<CanvasScaler>();
        }

        private void Resolution()
        {
            var fixedAspectRatio = 16f / 9;
            var curAspectRatio = (float)Screen.width / Screen.height;
            if (curAspectRatio > fixedAspectRatio)
            {
                _canvas.matchWidthOrHeight = 1;
            }
            else if (curAspectRatio < fixedAspectRatio) _canvas.matchWidthOrHeight = 0;
        }
    }
}