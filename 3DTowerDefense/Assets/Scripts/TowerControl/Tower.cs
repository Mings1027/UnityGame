using System;
using System.Threading;
using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;

        private bool _isBuilt;
        private bool _isTargeting;
        private bool _isUpgrading;
        private CancellationTokenSource _cts;

        public TowerManager towerManager;
        public int towerLevel;

        protected Transform Target;

        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponent<MeshFilter>();
            targets = new Collider[5];
        }

        //==================================Event function=====================================================

        protected virtual void OnDisable()
        {
            _isBuilt = false;
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            towerLevel = -1;
            _meshFilter.mesh = towerManager.towerLevels[0].towerMesh;
        }

        private void FixedUpdate()
        {
            if (!_isBuilt || !_isTargeting || _isUpgrading) return;
            if (towerManager.towerLevels[towerLevel].IsCoolingDown) return;
            Attack();
            towerManager.towerLevels[towerLevel].StartCoolDown();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _outline.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _outline.enabled = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        //==================================Event function=====================================================
        //==================================Custom function====================================================
        public void Init()
        {
            _isBuilt = true;
            _isUpgrading = false;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            Upgrade();
        }

        protected abstract void Attack();

        private void UpdateTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, range, targets, enemyLayer);
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

            if (nearestEnemy != null && shortestDistance <= range)
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

        public void Upgrade()
        {
            if (_isUpgrading) return;
            if (towerLevel >= 3) return;
            UpgradeAsync().Forget();
        }

        public void UniqueUpgrade()
        {
            if (_isUpgrading) return;
            UniqueUpgradeAsync().Forget();
        }

        private async UniTaskVoid UpgradeAsync()
        {
            LevelUpStart();
            await UniTask.Delay(TimeSpan.FromSeconds(towerManager.towerLevels[towerLevel].constructionTime),
                cancellationToken: _cts.Token);
            LevelUpEnd();
            range = towerManager.towerLevels[towerLevel].attackRange;
        }

        private async UniTaskVoid UniqueUpgradeAsync()
        {
            towerLevel += 2;
            _meshFilter.mesh = towerManager.towerLevels[towerLevel].consMesh;
            _isUpgrading = true;
            await UniTask.Delay(TimeSpan.FromSeconds(towerManager.towerLevels[towerLevel].constructionTime),
                cancellationToken: _cts.Token);
            _meshFilter.mesh = towerManager.towerLevels[towerLevel].towerMesh;
        }

        protected virtual void LevelUpStart()
        {
            towerLevel++;
            _meshFilter.mesh = towerManager.towerLevels[towerLevel].consMesh;
            _isUpgrading = true;
        }

        protected virtual void LevelUpEnd()
        {
            _meshFilter.mesh = towerManager.towerLevels[towerLevel].towerMesh;
            _isUpgrading = false;
        }

        //==================================Custom function====================================================
    }
}