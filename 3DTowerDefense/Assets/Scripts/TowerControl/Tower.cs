using System;
using System.Threading;
using AttackControl;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;
        private bool _isUpgrading;

        protected bool isSold;
        protected CancellationTokenSource cts;

        public event Action<MeshFilter> onResetMeshEvent;
        public event Action<Tower, Transform> onOpenTowerEditPanelEvent;

        public enum TowerType
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public TowerType Type => towerType;

        protected int towerLevel;

        public int TowerLevel => towerLevel;
        public float TowerRange { get; private set; }
        public bool IsSelected { get; set; }

        [SerializeField] private TowerType towerType;

        protected virtual void Awake()
        {
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
            towerLevel = -1;
            isSold = true;
            cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
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

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_isUpgrading) return;
            onOpenTowerEditPanelEvent?.Invoke(this, transform);
        }

        //==================================Custom function====================================================
        //======================================================================================================

        public void TowerLevelUp(int uniqueLevel)
        {
            if (towerLevel < 2) towerLevel++;
            else if (uniqueLevel > 0) towerLevel = uniqueLevel;
        }

        public virtual void ReadyToBuild(MeshFilter consMeshFilter)
        {
            _isUpgrading = true;
            _outline.enabled = false;
            _meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void Building(bool haveUnit, MeshFilter towerMeshFilter, float delay, float range, int damage,
            int health = 0)
        {
            _isUpgrading = false;
            _meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            TowerRange = range;
            if (haveUnit) return;

            GetComponent<TargetFinder>().SetUp(delay, damage, range, health);
        }
    }
}