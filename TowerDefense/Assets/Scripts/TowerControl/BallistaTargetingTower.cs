using DataControl;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public class BallistaTargetingTower : TargetingTower
    {
        private int _archerCount;
        private Vector3 _targetDirection;

        [SerializeField] private Transform ballista;
        [SerializeField] private Transform firePos;
        [SerializeField] private float smoothTurnSpeed;

        protected override void Init()
        {
            base.Init();
            effectName = new[] { "BallistaVfx1", "BallistaVfx2", "BallistaVfx3" };
        }

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
            var bullet = ObjectPoolManager.Get<BallistaProjectile>(StringManager.BallistaProjectile, firePos.position);
            bullet.Init(target, damage);

            EffectAttack(bullet.transform);
        }
    }
}