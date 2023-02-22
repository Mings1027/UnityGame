using System;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Outline _outline;

        private bool _isBuilt;
        private bool _isTargeting;
        private bool _isUpgrading;

        public int towerLevel;
        public int towerNum;
        public event Action<Tower> OnGetTowerInfoEvent;
        public MeshFilter meshFilter;
        public Cooldown cooldown;

        protected Transform Target;

        public float atkRange;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            meshFilter = GetComponentInChildren<MeshFilter>();
            targets = new Collider[5];
        }

        //==================================Event function=====================================================

        protected virtual void OnDisable()
        {
            _isBuilt = false;
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            towerLevel = -1;
            OnGetTowerInfoEvent = null;
        }

        private void FixedUpdate()
        {
            if (!_isBuilt || !_isTargeting || _isUpgrading) return;
            if (cooldown.IsCoolingDown) return;
            Attack();
            cooldown.StartCoolDown();
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
            OnGetTowerInfoEvent?.Invoke(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        //==================================Event function=====================================================
        //==================================Custom function====================================================
        public void Init()
        {
            _isBuilt = true;
            _isUpgrading = false;
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
        }

        protected abstract void Attack();

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
                Target = nearestEnemy;
                _isTargeting = true;
            }
            else
            {
                Target = null;
                _isTargeting = false;
            }
        }

        //==================================Custom function====================================================
    }
}