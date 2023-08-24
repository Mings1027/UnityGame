using UnityEngine;

namespace MapControl
{
    public class ExpandMapButton : MonoBehaviour
    {
        private Camera _cam;
        public Vector3 targetPos { get; set; }

        private void Awake()
        {
            _cam = Camera.main;
        }
        
        private void LateUpdate()
        {
            transform.position = _cam.WorldToScreenPoint(targetPos);
        }

        public void ExpandMap()
        {
            MapController.Instance.ExpandMap(targetPos);
            gameObject.SetActive(false);
        }
    }
}