using UnityEngine;

namespace UnitControl
{
    public class ArcherUnit : MonoBehaviour
    {
        private Transform _target;
        private bool _isTargeting;

        [SerializeField] private float smoothTurnSpeed;

        private void LateUpdate()
        {
            if (!_isTargeting) return;
            var targetPos = _target.position + _target.forward;
            var dir = targetPos - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var lookRot = Quaternion.Euler(0, yRot, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothTurnSpeed);
        }

        public void TargetUpdate(Transform t, bool isTargeting)
        {
            _target = t;
            _isTargeting = isTargeting;
        }
    }
}