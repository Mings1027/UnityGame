using UnityEngine;

namespace InterpolationCurves
{
    public class SecondOrder : MonoBehaviour
    {
        public Transform target;
        public float speed, rotationSpeed;
        [SerializeField] [Range(0, 1)] private float bounce;
        private Vector3 _oldPos;
        private Vector3 _curPos;
        private Vector3 _nextPos;

        private void FixedUpdate()
        {
            // OrderSystem();
            Second();
        }

        private void OrderSystem()
        {
            var transformPosition = transform.position;
            var curPos = transformPosition;
            transformPosition = Vector3.Lerp(transformPosition, target.position,
                (1 - bounce) * speed * Time.fixedDeltaTime);
            transformPosition += bounce * (transformPosition - _oldPos);
            _oldPos = curPos;
            transform.SetPositionAndRotation(transformPosition,
                Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.fixedDeltaTime));
        }

        private void Second()
        {
            _curPos = transform.position;
            _nextPos = _curPos;
            _curPos = Vector3.Lerp(_curPos, target.position,
                (1 - bounce) * speed * Time.fixedDeltaTime);

            _curPos = (1 + bounce) * _curPos - bounce * _oldPos;
            transform.position = _curPos;
            _oldPos = _nextPos;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_curPos, 0.1f);
        }
    }
}