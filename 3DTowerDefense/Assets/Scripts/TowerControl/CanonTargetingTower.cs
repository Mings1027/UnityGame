using System;
using DataControl;
using DG.Tweening;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class CanonTargetingTower : TargetingTower
    {
        private Sequence _atkSequence;
        private Transform[] _singleShootPoints;
        private Transform[] _multiShootPoints;
        
        [SerializeField] private MeshFilter[] canonMeshFilters;

        protected override void OnEnable()
        {
            base.OnEnable();
            onAttackEvent += SingleShoot;
        }

        private void OnDestroy()
        {
            _atkSequence.Kill();
        }

        protected override void Init()
        {
            base.Init();
            targetColliders = new Collider[5];
            var singlePoint = GameObject.Find("SingleShootPoint").transform;
            _singleShootPoints = new Transform[singlePoint.childCount];
            for (var i = 0; i < _singleShootPoints.Length; i++)
            {
                _singleShootPoints[i] = singlePoint.GetChild(i);
            }

            var multiPoint = GameObject.Find("MultiShootPoint").transform;
            _multiShootPoints = new Transform[multiPoint.childCount];
            for (var i = 0; i < _multiShootPoints.Length; i++)
            {
                _multiShootPoints[i] = multiPoint.GetChild(i);
            }

            _atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(meshFilter.transform.DOScaleY(0.5f, 0.3f).SetEase(Ease.OutQuint))
                .Append(meshFilter.transform.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuint));
        }

        public override void BuildTowerWithDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage, float attackRange,
            float attackDelay, float health = 0)
        {
            base.BuildTowerWithDelay(consMeshFilter, minDamage, maxDamage, attackRange, attackDelay, health);

            if (TowerUniqueLevel != 1) return;

            onAttackEvent = null;
            onAttackEvent += MultiShoot;
        }

        protected override void Attack()
        {
            base.Attack();
            _atkSequence.Restart();
        }

        private void SingleShoot()
        {
            Shoot(target, _singleShootPoints[TowerLevel].position + new Vector3(0, 1, 0));
        }

        private void MultiShoot()
        {
            for (var i = 0; i < 3; i++)
            {
                Shoot(target, _multiShootPoints[i].position + new Vector3(0, 1, 0));
            }
        }

        private void Shoot(Transform t, Vector3 pos)
        {
            ObjectPoolManager.Get(PoolObjectName.CanonShootSfx, transform);
            ObjectPoolManager.Get(PoolObjectName.CanonSmoke, pos);
            var m = ObjectPoolManager.Get<CanonProjectile>(PoolObjectName.CanonBullet, pos);
            m.Init(t.position, Damage);
            
            if (!m.TryGetComponent(out CanonProjectile u)) return;
            var level = IsUniqueTower ? TowerUniqueLevel + 3 : TowerLevel;
            u.CanonMeshFilter.sharedMesh = canonMeshFilters[level].sharedMesh;
        }
    }
}