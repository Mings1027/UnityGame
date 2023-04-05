using UnityEngine;

namespace TowerControl
{
    public class MageTower : TowerAttacker
    {
        private MeshFilter _crystalMeshFilter;
        private Transform[] _crystalPositions;
        
        [SerializeField] private Transform crystal;
        [SerializeField] private Transform crystalPosition;
        [SerializeField] private Mesh[] crystalMesh;
        
        protected override void Awake()
        {
            base.Awake();
            _crystalMeshFilter = crystal.GetComponent<MeshFilter>();

            _crystalPositions = new Transform[crystalPosition.childCount];
            for (var i = 0; i < _crystalPositions.Length; i++)
            {
                _crystalPositions[i] = crystalPosition.GetChild(i);
            }
        }

        public override void UnderConstruction(MeshFilter consMeshFilter)
        {
            base.UnderConstruction(consMeshFilter);
            crystal.position = transform.position;
        }

        public override void ConstructionFinished(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
        {
            base.ConstructionFinished(towerMeshFilter, minDamage, maxDamage, range, delay);
            crystal.position = _crystalPositions[TowerLevel].position;
            _crystalMeshFilter.sharedMesh = crystalMesh[TowerLevel];
        }

        protected override void Attack()
        {
            print("attttack");
        }
    }
}