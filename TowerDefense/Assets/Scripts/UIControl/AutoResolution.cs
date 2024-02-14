using UnityEngine;

namespace UIControl
{
    public class AutoResolution : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"width : {Screen.width}");
            Debug.Log($"height : {Screen.height}");
            SetResolution();
        }

        private void SetResolution()
        {
            var cam = Camera.main;
            var rect = cam.rect;

            var scaleHeight = (float)Screen.width / Screen.height / ((float)16 / 9);
            var scaleWidth = 1f / scaleHeight;

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
    }
}