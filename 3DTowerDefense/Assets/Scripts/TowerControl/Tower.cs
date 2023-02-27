using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using InterfaceControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler
    {
        protected CancellationTokenSource Cts;
        private Outline _outline;
        private MeshFilter _meshFilter;

        private bool _isBuilt;

        protected Vector3 direction;
        private bool isTargeting;
        private bool attackAble;
        private float atkRange;
        protected float atkDelay;
        protected Transform target;

        public enum TowerType
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public TowerType Type => towerType;
        [SerializeField] private TowerType towerType;

        public int towerLevel;
        public bool isUpgrading;

        public event Action<Tower, Vector3> OnOpenTowerEditPanelEvent;
        public event Action<MeshFilter> OnResetMeshEvent;

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
            Cts?.Dispose();
            Cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            attackAble = true;
        }

        protected virtual void OnDisable()
        {
            _isBuilt = false;
            towerLevel = -1;
            Cts.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            OnResetMeshEvent?.Invoke(_meshFilter);
            OnOpenTowerEditPanelEvent = null;
            OnResetMeshEvent = null;
        }

        private void Update()
        {
            if (!isTargeting || !attackAble) return;
            attackAble = false;
            Attack();
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
            OnOpenTowerEditPanelEvent?.Invoke(this, transform.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        //==================================Event function=====================================================
        //==================================Custom function====================================================

        public virtual void SetUp(float attackRange, float attackDelay)
        {
            _outline.enabled = false;
            _isBuilt = true;
            atkRange = attackRange;
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
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay));
            attackAble = true;
        }

        private void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, targets, enemyLayer);
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

            if (nearestEnemy != null && shortestDistance <= atkRange)
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

        //==================================Custom function====================================================
    }
}