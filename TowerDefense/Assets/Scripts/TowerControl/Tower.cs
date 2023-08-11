using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Collider _collider;
        private MeshFilter _initMesh;

        protected MeshFilter meshFilter;
        protected bool isUpgrading;
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
        public bool IsUnitTower => isUnitTower;
        public TowerTypeEnum towerTypeEnum => towerType;

        [SerializeField] private TowerTypeEnum towerType;
        [SerializeField] private bool isUnitTower;

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
            meshFilter.sharedMesh = _initMesh.sharedMesh;
            OnClickTower = null;
        }

        //==================================Custom Method====================================================
        //======================================================================================================

        protected virtual void Init()
        {
            _collider = GetComponent<Collider>();
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _initMesh = meshFilter;
        }

        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public virtual void TowerSetting(MeshFilter towerMesh, int damageData, int attackRangeData,
            float attackDelayData)
        {
            isUpgrading = false;
            meshFilter.sharedMesh = towerMesh.sharedMesh;
            _collider.enabled = true;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClickTower?.Invoke(this);
        }
    }
}