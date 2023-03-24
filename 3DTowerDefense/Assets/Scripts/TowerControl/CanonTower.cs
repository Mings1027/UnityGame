using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class CanonTower : TowerAttacker
    {
        private Transform[] _singleShootPoints;
        private Transform[] _multiShootPoints;

        [SerializeField] private MeshFilter[] canonMeshFilters;

        [SerializeField] private int min, max;

        protected override void Awake()
        {
            base.Awake();
            _singleShootPoints = new Transform[transform.GetChild(0).childCount];
            for (var i = 0; i < _singleShootPoints.Length; i++)
            {
                _singleShootPoints[i] = transform.GetChild(0).GetChild(i);
            }

            _multiShootPoints = new Transform[transform.GetChild(1).childCount];
            for (int i = 0; i < _multiShootPoints.Length; i++)
            {
                _multiShootPoints[i] = transform.GetChild(1).GetChild(i);
            }
        }

        protected override void Attack()
        {
            StackObjectPool.Get<AudioSource>("ShootMissileSound", transform.position);
            if (TowerLevel != 4)
                SingleShoot(targetFinder.Target.position);
            else
                MultiShoot(targetFinder.Target);
        }

        private void SingleShoot(Vector3 endPos)
        {
            var m = StackObjectPool.Get<Projectile>("UnitMissile", transform.position);
            m.SetPosition(_singleShootPoints[TowerLevel].position, endPos);
            if (m.TryGetComponent(out UnitMissile u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);
        }

        private void MultiShoot(Transform endPos)
        {
            for (var i = 0; i < 3; i++)
            {
                var m = StackObjectPool.Get<Projectile>("UnitMissile", transform.position);
                m.SetPosition(_multiShootPoints[i].position, endPos.position + endPos.forward * Random.Range(min, max));
                if (m.TryGetComponent(out UnitMissile u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);
            }
        }
    }
}