using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Collider _collider;
        private MeshFilter _initMesh;

        private int _minDamage, _maxDamage;

        protected float atkRange;
        protected int Damage => Random.Range(_minDamage, _maxDamage);
        protected LayerMask TargetLayer => targetLayer;
        protected MeshFilter meshFilter;
        protected bool isUpgrading;
        protected bool isSold;

        protected Collider[] targetColliders;

        // public event Action<Tower> onOpenTowerEditPanelEvent;

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
        public float TowerRange { get; private set; }

        [SerializeField] private Type towerType;
        [SerializeField] private LayerMask targetLayer;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _initMesh = meshFilter;
        }

        protected virtual void OnEnable()
        {
            isSold = false;
            InvokeRepeating(nameof(Targeting), 1, 1);
        }

        protected virtual void OnDisable()
        {
            TowerLevel = -1;
            TowerUniqueLevel = -1;
            IsUniqueTower = false;
            isSold = true;
            meshFilter.sharedMesh = _initMesh.sharedMesh;
            // onOpenTowerEditPanelEvent = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isUpgrading) return;
            // onOpenTowerEditPanelEvent?.Invoke(this);
        }

        //==================================Custom Method====================================================
        //======================================================================================================

        protected abstract void Targeting();
        
        public void TowerLevelUp()
        {
            TowerLevel++;
        }

        public void TowerUniqueLevelUp(int uniqueLevel)
        {
            IsUniqueTower = true;
            TowerUniqueLevel = uniqueLevel;
        }

        public virtual void TowerInit(MeshFilter consMeshFilter)
        {
            isUpgrading = true;
            meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void TowerSetting(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay, int health = 0)
        {
            isUpgrading = false;
            meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            TowerRange = range;
            _collider.enabled = true;

            _minDamage = minDamage;
            _maxDamage = maxDamage;
            atkRange = range;
        }
    }
}