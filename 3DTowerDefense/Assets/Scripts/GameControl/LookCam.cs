using UnityEngine;

namespace GameControl
{
    public class LookCam : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (_cam.transform.rotation == transform.rotation) return;

            transform.rotation = Quaternion.Euler(0f, _cam.transform.rotation.eulerAngles.y, 0f);
        }
    }
}
