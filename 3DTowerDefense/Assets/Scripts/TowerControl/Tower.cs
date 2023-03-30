using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        private Collider _collider;
        private Outline _outline;
        private MeshFilter _meshFilter;

        protected bool isUpgrading;
        protected bool isSold;

        public event Action<MeshFilter> onResetMeshEvent;
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

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }

        protected virtual void OnEnable()
        {
            isSold = false;
        }

        protected virtual void OnDisable()
        {
            TowerLevel = -1;
            isSold = true;
            onResetMeshEvent?.Invoke(_meshFilter);
            onResetMeshEvent = null;
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

        public void TowerLevelUp(int uniqueLevel)
        {
            if (TowerLevel < 2) TowerLevel++;
            else if (uniqueLevel > 0) TowerLevel = uniqueLevel;
        }

        public virtual void ReadyToBuild(MeshFilter consMeshFilter)
        {
            isUpgrading = true;
            _outline.enabled = false;
            _meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range, float delay)
        {
            isUpgrading = false;
            _meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            TowerRange = range;
            _collider.enabled = true;
        }
    }
}