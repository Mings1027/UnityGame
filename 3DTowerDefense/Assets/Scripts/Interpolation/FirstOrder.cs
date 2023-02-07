using System;
using UnityEngine;

namespace InterpolationCurves
{
    public class FirstOrder : MonoBehaviour
    {
        public float lerp;
        private Vector3 prevPos, curPos, nextPos;
        public bool isMoving;

        public Transform target;
        public float radius;
        public float speed;
        public AnimationCurve yCurve;

        private void Awake()
        {
            lerp = 1;
        }

        private void Update()
        {
            if (!isMoving && Vector3.Distance(transform.position, target.position) > radius)
            {
                lerp = 0;
                prevPos = transform.position;
                nextPos = target.position;
                isMoving = true;
            }

            if (lerp < 1 && isMoving)
            {
                curPos = Vector3.Lerp(prevPos,nextPos, lerp);
                curPos.y = yCurve.Evaluate(lerp);
                lerp += Time.deltaTime * speed;
                transform.position = curPos;
                if (lerp >= 1) isMoving = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}