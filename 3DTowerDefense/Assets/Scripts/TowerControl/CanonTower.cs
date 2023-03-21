using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class CanonTower : TowerAttacker
    {
        private Transform[] _shootPoints;
        [SerializeField] private MeshFilter[] canonMeshFilters;

        protected override void Awake()
        {
            base.Awake();
            _shootPoints = new Transform[transform.GetChild(0).childCount];
            for (var i = 0; i < _shootPoints.Length; i++)
            {
                _shootPoints[i] = transform.GetChild(0).GetChild(i);
            }
        }

        protected override void CheckState()
        {
            if (!targetFinder.IsTargeting || !targetFinder.attackAble) return;
            Attack();
            targetFinder.StartCoolDown().Forget();
        }

        protected override void Attack()
        {
            var t = targetFinder.Target.position;
            SpawnBullet(t);
        }

        private void SpawnBullet(Vector3 endPos)
        {
            var p = StackObjectPool.Get<Projectile>("UnitBullet", _shootPoints[TowerLevel].transform.position,
                transform.rotation);
            p.GetComponent<UnitBullet>().ChangeMesh(canonMeshFilters[TowerLevel]);
            p.Parabola(_shootPoints[TowerLevel].transform, endPos).Forget();
            p.damage = targetFinder.Damage;
        }
    }
}