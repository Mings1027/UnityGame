using System;
using System.Threading;
using BuildControl;
using Cysharp.Threading.Tasks;
using GameControl;
using InfoControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TowerControl
{
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Outline _outline;
        private MeshFilter _meshFilter;
        private BuildController _buildController;
        private TowerInfoSystem _towerInfoSystem;
        private MeshFilter _towerChildMeshFilter;
        private bool _isTargeting;
        private bool _isChild;
        private CancellationTokenSource _cts;

        public TowerLevelManager towerLevelManager;

        protected Transform Target;
        protected int TowerLevel;

        [SerializeField] private Cooldown cooldown;
        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;
        [SerializeField] private Transform infoPanelTransform;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponent<MeshFilter>();
            _buildController = BuildController.Instance;
            _towerInfoSystem = TowerInfoSystem.Instance;
            _isChild = transform.GetChild(0).TryGetComponent(out _towerChildMeshFilter);
            targets = new Collider[5];
        }

        //==================================Event function=====================================================
        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            UpgradeTower();
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            TowerLevel = -1;
            _meshFilter.mesh = towerLevelManager.towerLevels[0].towerMesh;
        }

        private void FixedUpdate()
        {
            if (cooldown.IsCoolingDown || !_isTargeting) return;
            Attack();
            cooldown.StartCoolDown();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _buildController.Tower = this;
            OpenTowerInfo();
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

        public void UpgradeTower()
        {
            if (TowerLevel >= towerLevelManager.towerLevels.Length - 1) return;
            Upgrade().Forget();
        }

        private async UniTaskVoid Upgrade()
        {
            if (_isChild) _towerChildMeshFilter.mesh = null;
            TowerLevel++;
            _meshFilter.mesh = towerLevelManager.towerLevels[TowerLevel].consMesh;

            await UniTask.Delay(TimeSpan.FromSeconds(towerLevelManager.towerLevels[TowerLevel].constructionTime),
                cancellationToken: _cts.Token);
            BatchUnit();
        }

        protected virtual void BatchUnit()
        {
            _meshFilter.mesh = towerLevelManager.towerLevels[TowerLevel].towerMesh;
            if (_isChild)
            {
                _towerChildMeshFilter.mesh = towerLevelManager.towerLevels[TowerLevel].childMesh;
            }
        }

        private void OpenTowerInfo()
        {
            _towerInfoSystem.OpenInfo(infoPanelTransform.position,
                towerLevelManager.towerLevels[TowerLevel].towerInfo,
                towerLevelManager.towerLevels[TowerLevel].towerName);
        }
        //==================================Custom function====================================================
    }
}