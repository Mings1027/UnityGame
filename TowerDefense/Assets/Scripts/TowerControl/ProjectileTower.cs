using System.Diagnostics;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class ProjectileTower : AttackTower
    {
        private Collider[] _targetColliders;
        private TowerState _towerState;

        protected byte effectIndex;
        protected bool isTargeting;
        protected Sequence atkSequence;
        protected Collider target;
        protected Transform firePos;
        protected PoolObjectKey projectileKey;

        [SerializeField] protected AudioClip audioClip;
        [SerializeField, Range(1, 5)] private byte targetColliderCount;

        [Conditional("UNITY_EDITOR")]
        private void OnDrawGizmos()
        {
            if (towerData == null) return;
            Gizmos.DrawWireSphere(transform.position, towerData.curRange);
        }
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

        public override void Init()
        {
            base.Init();
            effectIndex = 0;
            _targetColliders = new Collider[targetColliderCount];
            patrolCooldown.cooldownTime = 0.5f;
        }

        public override void SetTowerData(TowerData towerData)
        {
            base.SetTowerData(towerData);
            var targetTowerData = (TargetingTowerData)towerData;
            projectileKey = targetTowerData.poolObjectKey;
        }

        public override void TowerPause()
        {
            base.TowerPause();
            _towerState = TowerState.Detect;
            target = null;
            isTargeting = false;
        }

        public override void TowerUpdate()
        {
            switch (_towerState)
            {
                case TowerState.Detect:
                    Detect();
                    break;
                case TowerState.Attack:
                    ReadyToAttack();
                    break;
            }
        }

#region Tower State

        protected virtual void Detect()
        {
            if (patrolCooldown.IsCoolingDown) return;
            var size = Physics.OverlapSphereNonAlloc(transform.position, towerData.curRange, _targetColliders,
                targetLayer);

            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = float.MaxValue;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _targetColliders[i].bounds.center);
                if (shortestDistance > distanceToResult)
                {
                    shortestDistance = distanceToResult;
                    target = _targetColliders[i];
                }
            }

            isTargeting = true;
            _towerState = TowerState.Attack;
            patrolCooldown.StartCooldown();
        }

        private void ReadyToAttack()
        {
            if (attackCooldown.IsCoolingDown) return;

            if (!target || !target.enabled ||
                Vector3.Distance(transform.position, target.bounds.center) > towerData.curRange)
            {
                _towerState = TowerState.Detect;
                return;
            }

            Attack();
            attackCooldown.StartCooldown();
            target = null;
            isTargeting = false;
            _towerState = TowerState.Detect;
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            SoundManager.Play3DSound(audioClip, transform.position);
            var projectile = PoolObjectManager.Get<Projectile>(projectileKey, firePos.position);
            projectile.Init(towerDamage, effectIndex, target);
        }

#endregion

        public override void TowerSetting(int damageData,
            float cooldownData, MeshFilter towerMesh)
        {
            base.TowerSetting(damageData, cooldownData, towerMesh);

            if (towerLevel % 2 == 0)
            {
                effectIndex++;
            }
        }

#endregion
    }
}