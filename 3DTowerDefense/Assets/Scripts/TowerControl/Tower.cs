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

        private bool _attackAble;
        private bool _isBuilt;
        private bool _isUpgrading;
        private Vector3 _checkRangePoint;

        protected CancellationTokenSource cts;
        protected int health;
        protected int damage;
        protected float atkRange;
        protected float atkDelay;
        protected int targetCount;
        protected Collider[] targets;

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
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask enemyLayer;


        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }

        protected virtual void Start()
        {
            targets = new Collider[targetCount];
        }

        //==================================Event function=====================================================
        protected virtual void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();

            InvokeRepeating(nameof(CheckTarget), 0f, 0.5f);
            _attackAble = true;
        }

        protected virtual void OnDisable()
        {
            _isBuilt = false;
            towerLevel = -1;
            cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            onResetMeshEvent?.Invoke(_meshFilter);
            onOpenTowerEditPanelEvent = null;
            onResetMeshEvent = null;
        }

        // private void FixedUpdate()
        // {
        //     if (!_attackAble || isUpgrading) return;
        //     Attack();
        //     if (!isTargeting) return;
        //     StartCoolDown().Forget();
        // }

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

        public virtual void Init(int unitHealth, int unitDamage, float attackDelay, float attackRange)
        {
            _isUpgrading = true;
            _outline.enabled = false;
            health = unitHealth;
            damage = unitDamage;
            atkDelay = attackDelay;
            atkRange = attackRange;
        }

        public virtual void SetUp()
        {
            _isUpgrading = false;
            _isBuilt = true;
        }

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            _attackAble = true;
        }

        private void CheckTarget()
        {
            if (!_isBuilt) return;
            Physics.OverlapSphereNonAlloc(_checkRangePoint, atkRange, targets, enemyLayer);
            if (_isUpgrading) return;
            UpdateTarget();
        }

        protected virtual void UpdateTarget()
        {
        }


        //==================================Custom function====================================================
    }
}