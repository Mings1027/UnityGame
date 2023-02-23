using System;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace TowerControl
{
    public class ArcherTower : Tower
    {
        [SerializeField] private Transform[] archerPos;
        private GameObject _archerUnit1;
        private GameObject _archerUnit2;
        private Vector3 _targetDirection;

        protected override void SpawnUnit()
        {
            if(_archerUnit1)_archerUnit1.SetActive(false);
            _archerUnit1 = StackObjectPool.Get("ArcherUnit", archerPos[towerLevel].position);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!_archerUnit1) return;
            _archerUnit1.SetActive(false);
            if (!_archerUnit2) return;
            _archerUnit2.SetActive(false);
        }

        protected override void Attack()
        {
            
        }
    }
}