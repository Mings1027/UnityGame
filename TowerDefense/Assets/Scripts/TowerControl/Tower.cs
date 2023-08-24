using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private BoxCollider boxCollider;
        private MeshFilter _defaultMesh;
        private MeshRenderer meshRenderer;

        protected MeshFilter meshFilter;

        // protected bool isUpgrading;
        protected bool isSold;
        protected bool isSpawn;

        public event Action<Tower> OnClickTower;

        public enum TowerTypeEnum
        {
            Ballista,
            Barracks,
            Canon,
            Mage,
            Defender
        }

        public int TowerLevel { get; private set; }
        public TowerTypeEnum towerTypeEnum => towerType;

        [SerializeField] private TowerTypeEnum towerType;

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
            OnClickTower?.Invoke(this);
        }
        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            boxCollider = GetComponent<BoxCollider>();
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            meshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            _defaultMesh = meshFilter;
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            meshFilter.sharedMesh = towerMesh.sharedMesh;
            boxCollider.enabled = true;

            ColliderSize();
        }

        private void ColliderSize()
        {
            var rendererY = meshRenderer.bounds.size.y;
            var size = boxCollider.size;
            size = new Vector3(size.x, rendererY, size.z);
            boxCollider.size = size;
        }
    }
}