using UnityEngine;

namespace GameControl
{
    public class LookCam : MonoBehaviour
    {
        private Camera _cam;
        private Renderer _renderer;

        private void Awake()
        {
            _cam = Camera.main;
            _renderer = GetComponent<Renderer>();
        }

        private void LateUpdate()
        {
            if (!GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(_cam), _renderer.bounds)) return;
            if (_cam.transform.rotation == transform.rotation) return;

            transform.rotation = _cam.transform.rotation;
        }
    }
}