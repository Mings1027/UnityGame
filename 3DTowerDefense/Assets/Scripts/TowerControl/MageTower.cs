using UnityEngine;

namespace TowerControl
{
    public class MageTower : TowerAttacker
    {
        private Transform _crystal;
        private MeshFilter _crystalMeshFilter;

        [SerializeField] private Mesh[] crystalMesh;
        [SerializeField] private Transform[] crystalPositions;

        protected override void Awake()
        {
            base.Awake();
            _crystal = transform.GetChild(1);
            _crystalMeshFilter = _crystal.GetComponent<MeshFilter>();
            var crystalPos = transform.GetChild(0);
            crystalPositions = new Transform[crystalPos.childCount];
            for (var i = 0; i < crystalPositions.Length; i++)
            {
                crystalPositions[i] = crystalPos.GetChild(i);
            }
        }

        public override void ReadyToBuild(MeshFilter consMeshFilter)
        {
            base.ReadyToBuild(consMeshFilter);
            _crystal.position = transform.position;
        }

        public override void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.Building(towerMeshFilter, minDamage, maxDamage, range, delay);
            _crystal.position = crystalPositions[TowerLevel].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[TowerLevel];
        }

        protected override void Attack()
        {
        }
    }
}