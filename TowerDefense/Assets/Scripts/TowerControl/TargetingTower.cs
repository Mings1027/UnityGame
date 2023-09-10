using System.Collections.Generic;
using GameControl;
using ProjectileControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class TargetingTower : Tower
    {
        private AudioSource _audioSource;
        private Collider[] _targetColliders;
        private int _effectIndex;
        private bool _isAttack;
        private Dictionary<string, string> _projectileHitDic;

        protected Transform target;
        protected int damage;
        protected bool isTargeting;
        protected string[] effectName;

        [SerializeField] private LayerMask targetLayer;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            _projectileHitDic = new Dictionary<string, string>();
            for (int i = 0; i < effectName.Length; i++)
            {
                _projectileHitDic.Add(effectName[i], "Hit" + effectName[i]);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _effectIndex = -1;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, TowerRange);
        }

        protected override void Init()
        {
            base.Init();
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

            if (isTargeting)
            {
                Attack();
                _audioSource.PlayOneShot(_audioSource.clip);
            }
        }

        protected abstract void Attack();

        protected void EffectAttack(Transform t)
        {
            var followBullet = ObjectPoolManager.Get<FollowProjectile>(effectName[_effectIndex], t);
            followBullet.target = t;
            followBullet.SetHitVfx(_projectileHitDic[effectName[_effectIndex]]);
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

            CancelInvoke();
            InvokeRepeating(nameof(Targeting), 1, attackDelayData);
        }
    }
}