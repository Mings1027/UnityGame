using System;
using System.Threading;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
        protected CancellationTokenSource cts;

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
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            TowerLevel = -1;
            isSold = true;
            cts?.Cancel();
            onResetMeshEvent?.Invoke(_meshFilter);
            onOpenTowerEditPanelEvent = null;
            onResetMeshEvent = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onOpenTowerEditPanelEvent?.Invoke(this, transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        //==================================Custom function====================================================
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

        public virtual void Building(MeshFilter towerMeshFilter, int minDamage, int maxDamage, float range, float delay,
            int health = 0)
        {
            isUpgrading = false;
            _meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            TowerRange = range;
            _collider.enabled = true;
        }
    }
}