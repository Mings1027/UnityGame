using UnityEngine;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour
    {
        private Collider _collider;
        private MeshFilter _initMesh;

        protected MeshFilter meshFilter;
        protected bool isUpgrading;
        protected bool isSold;

        public enum Type
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public Type TowerType => towerType;
        public bool IsUniqueTower { get; private set; }
        public int TowerLevel { get; private set; }
        public int TowerUniqueLevel { get; private set; }

        [SerializeField] private Type towerType;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            isSold = false;
        }

        protected virtual void OnDisable()
        {
            TowerLevel = -1;
            TowerUniqueLevel = -1;
            IsUniqueTower = false;
            isSold = true;
            meshFilter.sharedMesh = _initMesh.sharedMesh;
        }

        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _initMesh = meshFilter;
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public void TowerUniqueLevelUp(int uniqueLevel)
        {
            IsUniqueTower = true;
            TowerUniqueLevel = uniqueLevel;
        }

        public virtual void BuildTowerWithDelay(MeshFilter consMeshFilter, int minDamage, int maxDamage,
            float attackRange, float attackDelay, float health = 0)
        {
            isUpgrading = true;
            _collider.enabled = false;
            meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void BuildTower(MeshFilter towerMeshFilter)
        {
            isUpgrading = false;
            meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            _collider.enabled = true;
        }
    }
}