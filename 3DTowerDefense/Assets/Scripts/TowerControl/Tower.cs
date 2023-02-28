using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;

        private bool _attackAble;
        private float _atkRange;
        private Vector3 _checkRangePoint;

        protected bool isTargeting;
        protected float atkDelay;
        protected Transform target;
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
            _attackAble = true;
            CheckGround();
        }

        protected virtual void OnDisable()
        {
            towerLevel = -1;
            cts.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            onResetMeshEvent?.Invoke(_meshFilter);
            onOpenTowerEditPanelEvent = null;
            onResetMeshEvent = null;
        }

        private void FixedUpdate()
        {
            if (!_attackAble || isUpgrading) return;
            Attack();
            if (!isTargeting) return;
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

        public virtual void SetUp(float attackRange, float attackDelay)
        {
            _outline.enabled = false;
            _atkRange = attackRange;
            atkDelay = attackDelay;
            isUpgrading = false;
        }

        public void SetUpMesh(MeshFilter meshFilter)
        {
            _meshFilter.sharedMesh = meshFilter.sharedMesh;
        }

        protected abstract void Attack();

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