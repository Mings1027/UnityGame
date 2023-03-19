using AttackControl;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class CanonTower : Tower, IAttackTarget
    {
        private TargetFinder _targetFinder;
        private Transform _target;
        private bool _isTargeting;
        private Transform[] _shootPoints;

        protected override void Awake()
        {
            base.Awake();
            _targetFinder = GetComponent<TargetFinder>();
            _targetFinder.colliderSize = 5;
            _shootPoints = new Transform[transform.GetChild(0).childCount];
            for (var i = 0; i < _shootPoints.Length; i++)
            {
                _shootPoints[i] = transform.GetChild(0).GetChild(i);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InvokeRepeating(nameof(FindingTarget), 0, 1f);
        }

        private void Update()
        {
            if (!_isTargeting || !_targetFinder.attackAble) return;
            Attack();
            _targetFinder.StartCoolDown().Forget();
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay, health);
            GetComponent<TargetFinder>().SetUp(delay, range, minDamage, maxDamage);
        }

        private void FindingTarget()
        {
            if (isUpgrading)
            {
                _isTargeting = false;
                return;
            }

            var t = _targetFinder.FindClosestTarget();
            _target = t.Item1;
            _isTargeting = t.Item2;
        }

        public void Attack()
        {
            var t = _target.position;
            SpawnBullet(t);
        }

        private void SpawnBullet(Vector3 endPos)
        {
            var p = StackObjectPool.Get<Projectile>("UnitBullet", _shootPoints[TowerLevel].transform.position,
                transform.rotation);
            p.Parabola(_shootPoints[TowerLevel].transform, endPos).Forget();
            p.damage = _targetFinder.Damage;
        }
    }
}