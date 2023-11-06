using UnityEngine;

namespace TowerControl
{
    public class MoveUnitIndicator : MonoBehaviour
    {
        private Camera cam;
    
        public float TowerRange { get; set; }

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;

            var touch = Input.GetTouch(0);
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out var hit);
            if (hit.collider && hit.collider.CompareTag("Ground") &&
                Vector3.Distance(transform.position, hit.point) <= TowerRange)
            {
            
            }
        }
    }
}
