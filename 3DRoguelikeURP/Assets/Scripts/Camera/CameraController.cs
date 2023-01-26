using UnityEngine;
using UnityEngine.InputSystem;

namespace Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private InputActionReference rotQ, rotE;
        [SerializeField] private Transform target;

        [SerializeField] private float rotateSpeed;
        [SerializeField] private float rotateAngle;
        private Quaternion rotY;


        private void Start()
        {
            rotY = transform.rotation;
        }

        private void LateUpdate()
        {
            var transform1 = transform;
            transform1.position = target.position;
            Rotate();
        }

        // private void SmoothMove()
        // {
        //     var oldPos = transform.position;
        //     var curPos = Vector3.Lerp(oldPos, target.position, smoothSpeed);
        //     transform.position = curPos;
        // }
        private void Rotate()
        {
            if (rotQ.action.triggered) rotY *= Quaternion.AngleAxis(rotateAngle, Vector3.up);
            if (rotE.action.triggered) rotY *= Quaternion.AngleAxis(-rotateAngle, Vector3.up);

            transform.rotation = Quaternion.Lerp(transform.rotation, rotY, rotateSpeed * Time.deltaTime);
        }
    }
}