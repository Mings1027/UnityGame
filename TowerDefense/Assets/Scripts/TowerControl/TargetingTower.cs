using System;
using ManagerControl;
using PoolObjectControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _audioSource;
        private Collider[] _targetColliders;
        private bool _isAttack;
        private sbyte _effectIndex;
        private Cooldown _atkCooldown;
        private Cooldown _targetingCooldown;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;

        [SerializeField] private ParticleSystem.MinMaxGradient[] minMaxGradient;
        [SerializeField] private LayerMask targetLayer;

        private void Update()
        {
            if (!_targetingCooldown.IsCoolingDown)
            {
                Targeting();
            }

            if (!isTargeting || _atkCooldown.IsCoolingDown) return;
            Attack();
            _audioSource.Play();
            _atkCooldown.StartCooldown();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        /*=========================================================================================================================================
        *                                               Unity Event
        =========================================================================================================================================*/


        protected override void Init()
        {
            base.Init();
            _audioSource = GetComponent<AudioSource>();
            _targetingCooldown.cooldownTime = 2;
            _effectIndex = -1;
            _targetColliders = new Collider[3];
        }

        private void Targeting()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, TowerRange, _targetColliders, targetLayer);
            if (size <= 0)
            {
                target = null;
                isTargeting = false;
                return;
            }

            var shortestDistance = Mathf.Infinity;
            Transform nearestTarget = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestTarget = _targetColliders[i].transform;
            }

            target = nearestTarget;
            isTargeting = true;
        }

        protected abstract void Attack();

        protected virtual void ProjectileInit(PoolObjectKey poolObjKey, Vector3 firePos)
        {
            var bullet = PoolObjectManager.Get<Projectile>(poolObjKey, firePos);
            bullet.Init(damage, target, TowerType);
            bullet.ColorInit(ref minMaxGradient[_effectIndex]);
        }

        public override void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            base.TowerSetting(towerMesh, damageData, rangeData, attackDelayData);

            damage = damageData;

            if (TowerLevel % 2 == 0)
            {
                _effectIndex++;
            }

            _atkCooldown.cooldownTime = attackDelayData;

        }
    }
}