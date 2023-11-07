using UnityEngine;

namespace TowerControl
{
    public class MoveUnitIndicator : MonoBehaviour
    {
        private Camera _cam;
    
        public float TowerRange { get; set; }

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;

            var touch = Input.GetTouch(0);
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(transform.position, hit.point) <= TowerRange)
            {
            
            }
        }
    }
}
