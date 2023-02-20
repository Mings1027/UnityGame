using System;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace TowerControl
{
    public class CannonTower : Tower
    {
        private MeshFilter _canonMeshFilter;

        protected override void Awake()
        {
            base.Awake();
            _canonMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        }

        protected override void LevelUpStart()
        {
            base.LevelUpStart();
            _canonMeshFilter.mesh = null;
        }

        protected override void LevelUpEnd()
        {
            base.LevelUpEnd();
            _canonMeshFilter.mesh = towerManager.towerLevels[TowerLevel].childMesh;
        }

        protected override void Attack()
        {
        }
    }
}