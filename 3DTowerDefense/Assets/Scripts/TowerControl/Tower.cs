using System;
using System.Threading;
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

        private float _atkRange;
        private bool _attackAble;
        private bool _isBuilt;
        private Vector3 _checkRangePoint;

        protected bool isTargeting;
        protected Transform target;
        protected float atkDelay;
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

        public bool isUpgrading;

        public event Action<Tower, Vector3> onOpenTowerEditPanelEvent;
        public event Action<MeshFilter> onResetMeshEvent;

        [SerializeField] private TowerType towerType;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponentInChildren<MeshFilter>();
            targets = new Collider[5];
        }

        //==================================Event function=====================================================
        protected virtual void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            CheckGround();
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
            onOpenTowerEditPanelEvent?.Invoke(this, transform.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_checkRangePoint, _atkRange);
        }

        //==================================Event function=====================================================

        //==================================Custom function====================================================

        public virtual void Init(MeshFilter consMeshFilter)
        {
            _meshFilter.sharedMesh = consMeshFilter.sharedMesh;
        }

        public virtual void SetUp(MeshFilter towerMeshFilter, float attackRange, float attackDelay)
        {
            _isBuilt = true;
            _outline.enabled = false;
            _atkRange = attackRange;
            atkDelay = attackDelay;
            isUpgrading = false;
            _meshFilter.sharedMesh = towerMeshFilter.sharedMesh;
        }

        protected abstract void Targeting();

        private async UniTaskVoid StartCoolDown()
        {
            _attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: cts.Token);
            _attackAble = true;
        }

        private void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(_checkRangePoint, _atkRange, targets, enemyLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;
            for (var i = 0; i < size; i++)
            {
                var distanceToEnemy = Vector3.Distance(transform.position, targets[i].transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = targets[i].transform;
                }
            }

            if (nearestEnemy != null && shortestDistance <= _atkRange)
            {
                target = nearestEnemy;
                isTargeting = true;
            }
            else
            {
                target = null;
                isTargeting = false;
            }

            if (!_isBuilt) return;
            Targeting();
        }

        private void CheckGround()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100, groundLayer))
            {
                _checkRangePoint = hit.point;
            }
        }

        //==================================Custom function====================================================
    }
}