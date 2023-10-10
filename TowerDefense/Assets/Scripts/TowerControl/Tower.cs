using System;
using DataControl;
using InterfaceControl;
using UnityEngine;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IFingerUp
    {
        protected BoxCollider boxCollider;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        public event Action<Tower> OnClickTower;

        public float TowerRange { get; private set; }
        public sbyte TowerLevel { get; private set; }

        public TowerData TowerData => towerData;

        [SerializeField] private TowerData towerData;

        protected virtual void Awake()
        {
            Init();
        }

        //==================================Custom Method====================================================
        //======================================================================================================
        public abstract void TowerTargetInit();
        public abstract void TowerTargeting();
        public abstract void TowerUpdate();

        protected virtual void Init()
        {
            TowerLevel = -1;
            boxCollider = GetComponent<BoxCollider>();
            _meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, ushort damageData, byte rangeData,
            float attackDelayData)
        {
            TowerRange = rangeData;
            _meshFilter.sharedMesh = towerMesh.sharedMesh;
            boxCollider.enabled = true;

            ColliderSize();
        }

        private void ColliderSize()
        {
            var rendererY = _meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }

        public virtual void FingerUp()
        {
            OnClickTower?.Invoke(this);
        }
    }
}