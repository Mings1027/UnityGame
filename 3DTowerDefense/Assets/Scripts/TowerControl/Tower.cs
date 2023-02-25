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
        private enum TowerState
        {
            FirstLevel,
            MiddleLevel,
            MaxLevel
        }

        protected CancellationTokenSource Cts;
        private Outline _outline;

        private bool _isBuilt;
        private bool _isTargeting;
        private bool _isCoolingDown;

        private TowerState _state;

        public enum TowerType
        {
            Archer,
            Barracks,
            Canon,
            Mage
        }

        public TowerType Type => towerType;
        public MeshFilter meshFilter;
        public float atkDelay;
        public int towerLevel;
        public float atkRange;
        public bool isUpgrading;

        public event Action<Tower, Vector3> OnOpenTowerEditPanelEvent;

        protected Transform target;

        [SerializeField] private TowerType towerType;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            meshFilter = GetComponentInChildren<MeshFilter>();
            targets = new Collider[5];
        }

        //==================================Event function=====================================================
        private void OnEnable()
        {
            Cts?.Dispose();
            Cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        }

        protected virtual void OnDisable()
        {
            _isBuilt = false;
            towerLevel = -1;
            OnOpenTowerEditPanelEvent = null;
            Cts.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            if (!_isBuilt || !_isTargeting || isUpgrading) return;
            if (_isCoolingDown) return;
            _isCoolingDown = true;
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
            _state = towerLevel switch
            {
                < 2 => TowerState.FirstLevel,
                2 => TowerState.MiddleLevel,
                _ => TowerState.MaxLevel
            };

            OnOpenTowerEditPanelEvent?.Invoke(this, transform.position);
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        //==================================Event function=====================================================
        //==================================Custom function====================================================
        public virtual void Init()
        {
            _outline.enabled = false;
            _isBuilt = true;
        }

        private async UniTaskVoid StartCoolDown()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay));
            _isCoolingDown = false;
        }

        protected virtual void Attack()
        {
            
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
                _isTargeting = true;
            }
            else
            {
                target = null;
                _isTargeting = false;
            }
        }

        //==================================Custom function====================================================
    }
}