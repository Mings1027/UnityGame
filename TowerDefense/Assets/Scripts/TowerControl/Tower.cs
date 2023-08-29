using System;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private BoxCollider _boxCollider;
        private MeshFilter _defaultMesh;
        private MeshRenderer _meshRenderer;
        private Outline _outline;
        
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
        }

        protected virtual void OnDisable()
        {
            TowerLevel = -1;
            isSold = true;
            meshFilter.sharedMesh = _defaultMesh.sharedMesh;
            OnClickTower = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _outline.enabled = true;
            OnClickTower?.Invoke(this);
        }
        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            _boxCollider = GetComponent<BoxCollider>();
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _defaultMesh = meshFilter;

            _outline = GetComponent<Outline>();
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

        public void DisableOutline()
        {
            _outline.enabled = false;
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