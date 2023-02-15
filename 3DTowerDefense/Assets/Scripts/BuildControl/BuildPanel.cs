using UnityEngine;

namespace BuildControl
{
    public class BuildPanel : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            var rotation = _cam.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }
    }
}