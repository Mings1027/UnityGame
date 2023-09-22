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

        [SerializeField] private Transform ballista;
        [FormerlySerializedAs("firePos")] [SerializeField] private Transform fireTransform;
        [SerializeField] private float smoothTurnSpeed;

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var t = target.transform;
            var targetPos = t.position + t.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        protected override void Attack()
        {
            ProjectileInit(PoolObjectKey.BallistaProjectile, fireTransform.position);
        }

        protected override void ProjectileInit(PoolObjectKey poolObjKey, Vector3 firePos)
        {
            projectile = PoolObjectManager.Get<BallistaProjectile>(poolObjKey, firePos);
            base.ProjectileInit(poolObjKey, firePos);
        }
    }
}