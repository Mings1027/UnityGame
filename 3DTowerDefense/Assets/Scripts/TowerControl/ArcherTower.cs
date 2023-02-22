using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        [SerializeField] private Transform[] archerPos;
        private GameObject _archerUnit;
        private Vector3 _targetDirection;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!_archerUnit) return;
            _archerUnit.SetActive(false);
        }


        protected override void Attack()
        {
            var p = StackObjectPool.Get<Projectile>("ArcherArrow", archerPos[towerLevel].position);
            p.dir = Target.position - archerPos[towerLevel].position;
            p.lookVec = Target.position;
        }
    }
}