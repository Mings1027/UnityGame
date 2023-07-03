using UnityEngine;

namespace ManagerControl
{
    public class CameraAutoResolution : MonoBehaviour
    {
        private void Start()
        {
            SetResolution();
        }

        private void SetResolution()
        {
            var cam = GetComponent<Camera>();
            var rect = cam.rect;
            var scaleHeight = (float)Screen.width / Screen.height / ((float)16 / 9); // (가로 / 세로)
            var scaleWidth = 1f / scaleHeight;
            if (scaleHeight < 1)
            {
                rect.height = scaleHeight;
                rect.y = (1f - scaleHeight) / 2f;
            }
            else
            {
                // rect.width = scaleWidth;
                // rect.x = (1f - scaleWidth) / 2f;
                rect.width = 1;
                rect.x = 0;
            }

            cam.rect = rect;
        }

        private void OnPreCull() => GL.Clear(true, true, Color.black);
    }
}