using System;
using AttackControl;
using GameControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        private Collider _collider;
        private Outline _outline;
        private MeshFilter _initMesh;

        private int _minDamage, _maxDamage;

        protected float atkRange;
        protected int Damage => Random.Range(_minDamage, _maxDamage);
        protected LayerMask TargetLayer => targetLayer;
        protected MeshFilter meshFilter;
        protected bool isUpgrading;
        protected bool isSold;

        public event Action<Tower, Transform> onOpenTowerEditPanelEvent;

        public enum Type
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public Type TowerType => towerType;
        public int TowerLevel { get; private set; }
        public float TowerRange { get; private set; }

        [SerializeField] private Type towerType;
        [SerializeField] private LayerMask targetLayer;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
            _outline = GetComponent<Outline>();
            meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
            _initMesh = meshFilter;
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
            onOpenTowerEditPanelEvent = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isUpgrading) return;
            onOpenTowerEditPanelEvent?.Invoke(this, transform);
        }

        //==================================Custom Method====================================================
        //======================================================================================================

        protected abstract void FindingTarget();
        
        public void TowerLevelUp(int uniqueLevel)
        {
            if (TowerLevel < 2) TowerLevel++;
            else if (uniqueLevel > 0) TowerLevel = uniqueLevel;
        }

        public virtual void UnderConstruction(MeshFilter consMeshFilter)
        {
            isUpgrading = true;
            _outline.enabled = false;
            meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void ConstructionFinished(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range,
            float delay)
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