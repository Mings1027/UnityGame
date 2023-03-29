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
            StackObjectPool.Get("CanonShootSound", transform.position);
            if (TowerLevel != 4)
            {
                SingleShoot(target.position);
            }
            else
            {
                MultiShoot(target);
            }
        }

        private void SingleShoot(Vector3 endPos)
        {
            StackObjectPool.Get("CanonSmoke", _singleShootPoints[TowerLevel].position + new Vector3(0, 1, 0));
            var m = StackObjectPool.Get<Projectile>("CanonMissile", _singleShootPoints[TowerLevel].position);
            m.Setting("Ground", endPos, Damage);
            if (m.TryGetComponent(out Canon u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);
        }

        private void MultiShoot(Transform endPos)
        {
            for (var i = 0; i < 3; i++)
            {
                StackObjectPool.Get("CanonSmoke", _multiShootPoints[i].position + new Vector3(0, 1, 0));
                var m = StackObjectPool.Get<Projectile>("CanonMissile", _multiShootPoints[i].position);
                m.Setting("Ground", endPos.position + endPos.forward * Random.Range(min, max), Damage);
                if (m.TryGetComponent(out Canon u)) u.ChangeMesh(canonMeshFilters[TowerLevel]);
            }
        }
    }
}