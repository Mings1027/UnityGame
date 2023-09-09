using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private BoxCollider _boxCollider;
        private MeshRenderer _meshRenderer;
        
        protected MeshFilter meshFilter;
        protected bool isSold;

        public event Action<Tower> OnClickTower;
        
        public int TowerLevel { get; private set; }
        public TowerType towerTypeEnum => towerType;

        [SerializeField] private TowerType towerType;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            isSold = false;
            TowerLevel = -1;
        }

        protected virtual void OnDisable()
        {
            // OnClickTower = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClickTower?.Invoke(this);
        }
        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            _boxCollider = GetComponent<BoxCollider>();
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, int rangeData,
            float attackDelayData)
        {
            meshFilter.sharedMesh = towerMesh.sharedMesh;
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
    }
}