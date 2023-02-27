using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        private Unit _archerUnit1;
        private Unit _archerUnit2;
        private Vector3 _targetDirection;

        [SerializeField] private Transform[] archerPos;
        [SerializeField] private float projectileHeight;
        [SerializeField] private float projectileSpeed;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!_archerUnit1) return;
            _archerUnit1.gameObject.SetActive(false);
            if (!_archerUnit2) return;
            _archerUnit2.gameObject.SetActive(false);
        }

        public override void SetUp(float attackRange, float attackDelay)
        {
            base.SetUp(attackRange, attackDelay);
            SpawnUnit();
        }

        protected override void Attack()
        {
            var targetPosition = target.position;

            var archer1Pos = _archerUnit1.transform.position;
            direction = (targetPosition - archer1Pos).normalized;
            var p = StackObjectPool.Get<Projectile>("ArcherArrow", archer1Pos, Quaternion.LookRotation(direction));
            p.target = target;
            p.startPos = archer1Pos;
            if (!_archerUnit2) return;

            var archer2Pos = _archerUnit2.transform.position;
            direction = (targetPosition - archer2Pos).normalized;
            StackObjectPool.Get<Projectile>("ArcherArrow", archer2Pos, Quaternion.LookRotation(direction))
                .target = target;
        }

        private void SpawnUnit()
        {
            if (_archerUnit1) _archerUnit1.gameObject.SetActive(false);
            _archerUnit1 = StackObjectPool.Get<Unit>("ArcherUnit", archerPos[towerLevel].position);

            if (towerLevel != 4) return;
            if (_archerUnit2) _archerUnit2.gameObject.SetActive(false);
            _archerUnit2 = StackObjectPool.Get<Unit>("ArcherUnit", archerPos[towerLevel + 1].position);
        }
    }
}