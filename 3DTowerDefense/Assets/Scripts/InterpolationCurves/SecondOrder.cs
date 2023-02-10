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
        [SerializeField] private AnimationCurve legCurve;
        [SerializeField] private float stepDistance;
        [SerializeField] private float stopDistance;
        [SerializeField] private float stopTimer;
        [SerializeField] private float stopTime;
        private float _lerp;
        private bool _isMoving;

        private void Start()
        {
            _lerp = 1;
        }

        private void FixedUpdate()
        {
            // Check();
            // if (_isMoving)
            //     Second();
            CheckDistance();
            if (Vector3.Distance(target.position, transform.position) > stepDistance)
                Second();
        }

        private void CheckDistance()
        {
            var distanceFromTarget = Vector3.Distance(target.position, transform.position);
            if (!_isMoving && distanceFromTarget > stepDistance)
            {
                _isMoving = true;
                _lerp = 0;
            }

            if (_lerp >= 1) _isMoving = false;
        }

        private void Second()
        {
            var position = transform.position;
            _curPos = Vector3.Lerp(position, target.position, (1 - bounce) * speed * Time.fixedDeltaTime);
            _curPos += bounce * (_curPos - _oldPos);
            _oldPos = position;
            if (_lerp < 1)
            {
                _curPos.y += legCurve.Evaluate(_lerp);
                _lerp += Time.fixedDeltaTime * speed;
            }

            transform.position = _curPos;
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