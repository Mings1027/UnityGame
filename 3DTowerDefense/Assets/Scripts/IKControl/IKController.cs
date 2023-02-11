using UnityEngine;

namespace IKControl
{
    public class IKController : MonoBehaviour
    {
        private float _lerp;

        private RaycastHit _hit;
        private bool _isAllGrounded;
        private Vector3 _prevPos, _curPos, _nextPos;

        public LayerMask groundLayer;
        public Transform sensor;
        public IKController[] sideLeg;
        public float speed;
        public float stepDistance;
        public float stepHeight;

        private void Awake()
        {
            _prevPos = _curPos = _nextPos = transform.position;
            _lerp = 1;
        }

        private void FixedUpdate()
        {
            transform.position = _curPos;
            if (Physics.Raycast(sensor.position, Vector3.down, out _hit, 100, groundLayer))
            {
                if (Vector3.Distance(_nextPos, _hit.point) > stepDistance && sideLeg[0].IsGrounded() &&
                    sideLeg[1].IsGrounded())
                {
                    _isAllGrounded = true;
                    _lerp = 0;
                    _nextPos = _hit.point;
                    _prevPos = transform.position;
                }
            }

            if (_lerp < 1 && _isAllGrounded)
            {
                _curPos = Vector3.Lerp(_prevPos, _nextPos, _lerp);
                _curPos.y += Mathf.Sin(_lerp * Mathf.PI) * stepHeight;
                _lerp += Time.deltaTime * speed;
            }
        }

        public bool IsGrounded() => _lerp >= 1;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_nextPos, 0.1f);
            Gizmos.DrawLine(sensor.position, transform.position);
        }
    }
}