using System;
using InterfaceControl;
using ManagerControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IFingerUp
    {
        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        public event Action<Tower> OnClickTower;

        public bool isSpawn { get; set; }
        public float TowerRange { get; private set; }
        public int TowerLevel { get; private set; }
        // public string TowerName { get; private set; }

        public TowerType TowerType => towerType;
        [SerializeField] private TowerType towerType;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            TowerLevel = -1;
        }

        protected virtual void OnDisable()
        {
            // OnClickTower = null;
        }

        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            // TowerName = towerType.ToString();
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            TowerRange = rangeData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;
            _boxCollider.enabled = true;

            ColliderSize();
        }

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = _boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            _boxCollider.size = size;
        }

        public virtual void FingerUp()
        {
            OnClickTower?.Invoke(this);
        }
    }
}