using CustomEnumControl;
using DataControl.TowerData;
using DG.Tweening;
using GameControl;
using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : AttackTower
    {
        private Cooldown _patrolCooldown;
        protected sbyte effectIndex;
        protected Collider[] targetColliders;
        protected TowerState towerState;
        protected LayerMask targetLayer;
        protected bool isTargeting;
        protected Sequence atkSequence;
        protected Collider target;
        protected Transform firePos;

        [SerializeField] protected AudioClip audioClip;
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
            effectIndex = -1;
            targetColliders = new Collider[3];
            _patrolCooldown.cooldownTime = 0.5f;
        }

        public override void TowerTargetInit()
        {
            towerState = TowerState.Detect;
            target = null;
            isTargeting = false;
        }

        public override void TowerUpdate()
        {
            switch (towerState)
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
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            if (_patrolCooldown.IsCoolingDown) return;

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
            if (!attackCooldown.IsCoolingDown)
                towerState = TowerState.Attack;
            _patrolCooldown.StartCooldown();
        }

        protected virtual void ReadyToAttack()
        {
            if (!target || !target.enabled)
            {
                towerState = TowerState.Detect;
                return;
            }

            if (attackCooldown.IsCoolingDown) return;

            Attack();
            attackCooldown.StartCooldown();
            towerState = TowerState.Detect;
        }

        protected virtual void Attack()
        {
            atkSequence.Restart();
            SoundManager.Instance.Play3DSound(audioClip, transform.position);
            var targetingTowerData =
                (TargetingTowerData)UIManager.Instance.TowerDataPrefabDictionary[TowerType].towerData;
            var projectile =
                PoolObjectManager.Get<Projectile>(targetingTowerData.PoolObjectKey, firePos.position);

            projectile.ColorInit(effectIndex);
            projectile.Init(Damage, target);
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