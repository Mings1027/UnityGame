using CustomEnumControl;
using DataControl;
using DG.Tweening;
using GameControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private Cooldown _patrolCooldown;
        protected sbyte effectIndex;
        protected Collider[] targetColliders;
        protected AudioSource attackSound;
        protected TowerState towerState;
        protected LayerMask targetLayer;
        protected bool isTargeting;
        protected Sequence atkSequence;
        protected Collider target;
        protected Transform firePos;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }
#endif
        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/

        #region Unity Event

        private void OnDestroy()
        {
            atkSequence?.Kill();
        }

        #endregion

        #region Override Function

        protected override void Init()
        {
            base.Init();
            targetLayer = LayerMask.GetMask("Monster") | LayerMask.GetMask("FlyingMonster");
            attackSound = GetComponent<AudioSource>();
            effectIndex = -1;
            targetColliders = new Collider[3];
            _patrolCooldown.cooldownTime = 0.5f;
        }

        public override void TowerTargetInit()
        {
            towerState = TowerState.Patrol;
            target = null;
            isTargeting = false;
        }

        public override void TowerUpdate()
        {
            switch (towerState)
            {
                case TowerState.Patrol:
                    Patrol();
                    break;
                case TowerState.Attack:
                    AttackAsync();
                    break;
            }
        }

        #region Tower State

        protected virtual void Patrol()
        {
            if (_patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                target = targetColliders[i];
            }

            isTargeting = true;
            towerState = TowerState.Attack;
            _patrolCooldown.StartCooldown();
        }

        private void AttackAsync()
        {
            if (!target || !target.enabled)
            {
                towerState = TowerState.Patrol;
                return;
            }

            if (cooldown.IsCoolingDown) return;

            Attack();
            cooldown.StartCooldown();
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            attackSound.Play();
            var targetingTowerData = (TargetingTowerData)TowerData;
            var projectile =
                PoolObjectManager.Get<Projectile>(targetingTowerData.PoolObjectKey, firePos.position);

            projectile.ColorInit(effectIndex);
            projectile.Init(Damage, target);
            projectile.ProjectileUpdate().Forget();
        }

        #endregion

        public override void TowerSetting(MeshFilter towerMesh, int damageData, byte rangeData,
            ushort rpmData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, rpmData);

            if (TowerLevel % 2 == 0)
            {
                effectIndex++;
            }
        }

        #endregion
    }
}