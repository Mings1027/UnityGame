using DataControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private int _archerCount;
        private Vector3 _targetDirection;
        private Transform _ballista;
        private Transform _fireTransform;
        
        [SerializeField] private float smoothTurnSpeed;

        protected override void Awake()
        {
            base.Awake();
            _ballista = transform.Find("Ballista");
            _fireTransform = _ballista.GetChild(0);
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var t = target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            _ballista.rotation = Quaternion.Slerp(_ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Attack()
        {
            ProjectileInit(PoolObjectKey.BallistaProjectile, _fireTransform.position);
        }
    }
}