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
        private int effectsCount;

        [SerializeField] private Transform ballista;
        [SerializeField] private Transform firePos;
        [SerializeField] private float smoothTurnSpeed;

        protected override void OnDisable()
        {
            base.OnDisable();
            effectsCount = 0;
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var targetPos = target.position + target.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            if (TowerLevel % 2 == 0)
            {
                effectsCount++;
            }
        }

        protected override void Attack()
        {
            ObjectPoolManager.Get(PoolObjectName.BallistaShootSfx, transform);
            var bullet = ObjectPoolManager.Get<BallistaProjectile>(PoolObjectName.BallistaProjectile, firePos.position);
            bullet.Init(target, damage, effectsCount);
        }
    }
}