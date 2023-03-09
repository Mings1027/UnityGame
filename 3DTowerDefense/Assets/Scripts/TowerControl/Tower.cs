using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using InterfaceControl;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;
        private RaycastHit _hit;
        private bool _isUpgrading;
        private bool _attackAble;
        private float atkDelay;

        protected TargetFinder targetFinder;
        protected bool isSold;
        protected int damage;

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
        public Transform target;
        public bool isTargeting;

        public event Action<Tower, Vector3> onOpenTowerEditPanelEvent;
        public event Action<MeshFilter> onResetMeshEvent;

        [SerializeField] private TowerType towerType;

//==================================Event function=====================================================

        protected virtual void Awake()
        {
            targetFinder = GetComponent<TargetFinder>();
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }

        protected virtual void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            _attackAble = true;
            InvokeRepeating(nameof(FindTarget), 0, 1f);
        }

        protected virtual void OnDisable()
        {
            towerLevel = -1;
            cts?.Cancel();
            StackObjectPool.ReturnToPool(gameObject);
            onResetMeshEvent?.Invoke(_meshFilter);
            onOpenTowerEditPanelEvent = null;
            onResetMeshEvent = null;
            CancelInvoke();
        }

        protected virtual void Update()
        {
            if (!_attackAble || !isTargeting) return;
            UnitControl();
            StartCoolDown().Forget();
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

        protected virtual void OnDrawGizmos()
        {
            
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

        public virtual void SetUp(float attackDelay, int unitDamage, int unitHealth)
        {
            _isUpgrading = false;
            atkDelay = attackDelay;
            damage = unitDamage;
        }

        protected abstract void UnitControl();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            _attackAble = true;
        }

        //==================================Custom function====================================================
        protected virtual void FindTarget()
        {
            var t = targetFinder.FindClosestTarget();
            target = t.Item1;
            isTargeting = t.Item2;
        }
    }
}