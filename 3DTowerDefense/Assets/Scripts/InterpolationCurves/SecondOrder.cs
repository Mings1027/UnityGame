using System;
using UnityEngine;

namespace InterpolationCurves
{
    public class SecondOrder : MonoBehaviour
    {
        public Transform target;
        public float speed, rotationSpeed;
        [SerializeField] [Range(0, 1)] private float bounce;
        [SerializeField] private AnimationCurve curve;
        public Vector3 oldPos;

        private Vector3 _nextPos;

        private void FixedUpdate()
        {
            OrderSystem();
            // test();
        }

        private void OrderSystem()
        {
            var transformPosition = transform.position;
            var curPos = transformPosition;
            transformPosition = Vector3.Lerp(transformPosition, target.position,
                (1 - bounce) * speed * Time.fixedDeltaTime);
            transformPosition += bounce * (transformPosition - oldPos);
            oldPos = curPos;
            transform.SetPositionAndRotation(transformPosition,
                Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.fixedDeltaTime));
        }

        private void test()
        {
            var thisTransform = transform;
            var transformPosition = thisTransform.position;
            var transformRotation = thisTransform.rotation;

            var curPos = Vector3.Lerp(transformPosition, target.position, (1 - bounce) * speed * Time.fixedDeltaTime);
            var curRot = Quaternion.Slerp(transformRotation, target.rotation, rotationSpeed * Time.fixedDeltaTime);

            curPos += bounce * (transformPosition - oldPos);
            oldPos = transformPosition;
            transform.SetPositionAndRotation(curPos, curRot);
        }
    }
}