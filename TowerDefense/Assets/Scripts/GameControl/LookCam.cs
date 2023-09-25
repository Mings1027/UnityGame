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

            transform.rotation = _cam.transform.rotation;
        }
    }
}