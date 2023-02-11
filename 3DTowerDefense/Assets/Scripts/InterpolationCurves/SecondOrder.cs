using System;
using Unity.VisualScripting;
using UnityEngine;

namespace InterpolationCurves
{
    public class SecondOrder : MonoBehaviour
    {
        private Vector3 _oldPos;
        private Vector3 _curPos;
        private Vector3 _nextPos;

        [SerializeField] private Transform target;
        [SerializeField] private float speed;
        [SerializeField] [Range(0, 1)] private float bounce;
        [SerializeField] private float rotationSpeed;

        private void FixedUpdate()
        {
            Second();
        }


        private void Second()
        {
            var position = transform.position;
            _curPos = Vector3.Lerp(position, target.position, (1 - bounce) * speed * Time.fixedDeltaTime);
            _curPos += bounce * (_curPos - _oldPos);
            _oldPos = position;
            transform.SetPositionAndRotation(_curPos,
                Quaternion.Slerp(transform.rotation, target.rotation,
                    (1 - bounce) * rotationSpeed * Time.fixedDeltaTime));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}