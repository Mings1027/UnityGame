using UnityEngine;

namespace CameraControl
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform target;

        private void LateUpdate()
        {
            transform.position = target.position;
        }
    }
}