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
        private int effectsIndex;

        [SerializeField] private Transform ballista;
        [SerializeField] private Transform firePos;
        [SerializeField] private float smoothTurnSpeed;

        protected override void OnDisable()
        {
            base.OnDisable();
            effectsIndex = 0;
        }

        private void LateUpdate()
        {
            if (!isTargeting) return;

            var targetPos = target.position + target.forward;
            var dir = targetPos - transform.position;
            var rotGoal = Quaternion.LookRotation(dir) * Quaternion.Euler(-30f, 0f, 0f);
            ballista.rotation = Quaternion.Slerp(ballista.rotation, rotGoal, smoothTurnSpeed);
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, attackRangeData, attackDelayData);

            if (TowerLevel != 0 && TowerLevel % 2 == 0)
            {
                effectsIndex++;
            }
        }

        protected override void Attack()
        {
            ObjectPoolManager.Get(PoolObjectName.BallistaShootSfx, transform);
            var bullet = ObjectPoolManager.Get<BallistaProjectile>(PoolObjectName.BallistaProjectile, firePos.position);
            bullet.Init(target, damage);

            for (int i = 0; i <= effectsIndex; i++)
            {
                ObjectPoolManager.Get<FollowProjectile>(PoolObjectName.ballistaVfx[i], transform).target =
                    bullet.transform;
            }
        }
    }
}