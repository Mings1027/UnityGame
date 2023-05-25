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
        private Sequence atkSequence;
        private Transform[] _singleShootPoints;
        private Transform[] _multiShootPoints;

        private Action<Transform> onAttackEvent;

        [SerializeField] private MeshFilter[] canonMeshFilters;

        protected override void Awake()
        {
            base.Awake();
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

            atkSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(meshFilter.transform.DOScaleY(0.5f, 0.3f).SetEase(Ease.OutQuint))
                .Append(meshFilter.transform.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuint));
        }

        private void OnDestroy()
        {
            atkSequence.Kill();
        }

        public override void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.TowerSetting(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            onAttackEvent = null;
            onAttackEvent += TowerUniqueLevel != 1 ? SingleShoot : MultiShoot;
        }

        protected override void Attack()
        {
            atkSequence.Restart();
            onAttackEvent?.Invoke(target);
        }

        private void SingleShoot(Transform endPos)
        {
            Shoot(endPos, _singleShootPoints[TowerLevel].position + new Vector3(0, 1, 0));
        }

        private void MultiShoot(Transform endPos)
        {
            for (var i = 0; i < 3; i++)
            {
                Shoot(endPos, _multiShootPoints[i].position + new Vector3(0, 1, 0));
            }
        }

        private void Shoot(Transform t, Vector3 pos)
        {
            StackObjectPool.Get(PoolObjectName.CanonShootSfx, transform);
            StackObjectPool.Get(PoolObjectName.CanonSmoke, pos);
            var m = StackObjectPool.Get<Projectile>(PoolObjectName.CanonBullet, pos);
            m.Init(t, Damage);
            if (!m.TryGetComponent(out CanonProjectile u)) return;
            var level = IsUniqueTower ? TowerUniqueLevel + 3 : TowerLevel;
            u.CanonMeshFilter.sharedMesh = canonMeshFilters[level].sharedMesh;
        }
    }
}