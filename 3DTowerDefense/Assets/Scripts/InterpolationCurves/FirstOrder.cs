using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace InterpolationCurves
{
    public class FirstOrder : MonoBehaviour
    {
        private Vector3 _curPos, _nextPos, _oldPos;

        public Transform target;
        public float lerp;
        public float speed;
        public AnimationCurve legCurve;
        public float stepDistance;
        public float stopDistance;
        public bool isMoving;

        private void Start()
        {
            lerp = 1;
        }

        private void FixedUpdate()
        {
            if (!isMoving && Vector3.Distance(target.position, transform.position) > stepDistance)
            {
                lerp = 0;
                isMoving = true;
            }
            else if (Vector3.Distance(target.position, transform.position) < stopDistance)
            {
                isMoving = false;
            }

            if (isMoving || lerp < 1)
            {
                _oldPos = transform.position;
                _curPos = Vector3.Lerp(_oldPos, target.position, lerp);
                _curPos.y = legCurve.Evaluate(lerp);
                lerp += Time.fixedDeltaTime * speed;
                transform.position = _curPos;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, stepDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }
    }
}