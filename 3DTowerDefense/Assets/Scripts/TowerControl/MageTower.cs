using GameControl;
using UnityEngine;

namespace TowerControl
{
    public class MageTower : Tower
    {
        private MeshFilter _crystalMeshFilter;

        protected override void Awake()
        {
            base.Awake();
            _crystalMeshFilter = transform.GetChild(0).GetComponentInChildren<MeshFilter>();
        }

        protected override void LevelUpStart()
        {
            base.LevelUpStart();
            _crystalMeshFilter.mesh = null;
        }

        protected override void LevelUpEnd()
        {
            base.LevelUpEnd();
            _crystalMeshFilter.mesh = towerManager.towerLevels[TowerLevel].childMesh;
        }

        protected override void Attack()
        {
        }
    }
}