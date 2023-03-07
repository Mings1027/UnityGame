using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;
        private RaycastHit _hit;
        private bool _isUpgrading;
        private Vector3 _checkRangePoint;

        protected CancellationTokenSource cts;

        public enum TowerType
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public TowerType Type => towerType;

        public int towerLevel;

        public event Action<Tower, Vector3> onOpenTowerEditPanelEvent;
        public event Action<MeshFilter> onResetMeshEvent;

        [SerializeField] private TowerType towerType;

//==================================Event function=====================================================

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }

        protected virtual void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            towerLevel = -1;
            cts?.Cancel();
            CancelInvoke();
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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isUpgrading) return;
            onOpenTowerEditPanelEvent?.Invoke(this, transform.position);
        }

        //==================================Event function=====================================================

        //==================================Custom function====================================================

        public void ChangeMesh(MeshFilter consMeshFilter)
        {
            _meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void UnitInit()
        {
            _isUpgrading = true;
            _outline.enabled = false;
        }

        public virtual void SetUp(int unitHealth, int unitDamage, float attackDelay, float attackRange)
        {
            _isUpgrading = false;
        }

        //==================================Custom function====================================================
    }
}