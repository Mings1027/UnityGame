using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnitControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class TowerBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;
        private bool _isUpgrading;

        protected bool isSold;
        protected CancellationTokenSource cts;
        
        public event Action<MeshFilter> onResetMeshEvent;
        public event Action<TowerBase, Vector3> onOpenTowerEditPanelEvent;

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

        [SerializeField] private TowerType towerType;
        [SerializeField] private bool hasUnit;
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
            onOpenTowerEditPanelEvent?.Invoke(this, transform.position);
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

        public virtual void Building(float delay, float range, int damage, int health, MeshFilter towerMeshFilter)
        {
            _isUpgrading = false;
            _meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
            TowerRange = range;
            if (hasUnit) return;

            GetComponent<TargetFinder>().SetUp(delay, range, damage, health);
        }
    }
}