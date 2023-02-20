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
    public abstract class Tower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        private Camera _cam;
        private Outline _outline;
        private MeshFilter _meshFilter;
        private EditCanvasController _editCanvasController;

        private bool _isTargeting;
        private bool _isUpgrading;
        private CancellationTokenSource _cts;

        public TowerManager towerManager;

        protected Transform Target;
        protected int TowerLevel;

        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider[] targets;

        protected virtual void Awake()
        {
            _cam = Camera.main;
            _outline = GetComponent<Outline>();
            _meshFilter = GetComponent<MeshFilter>();
            _editCanvasController = EditCanvasController.Instance as EditCanvasController;
            targets = new Collider[5];
        }

        //==================================Event function=====================================================
        private void OnEnable()
        {
            _isUpgrading = false;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            Upgrade();
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            TowerLevel = -1;
            _meshFilter.mesh = towerManager.towerLevels[0].towerMesh;
        }

        private void FixedUpdate()
        {
            if (towerManager.towerLevels[TowerLevel].IsCoolingDown || !_isTargeting) return;
            Attack();
            towerManager.towerLevels[TowerLevel].StartCoolDown();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _editCanvasController.SelectedTower = this;
            OpenEditPanel();
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

        public void Upgrade()
        {
            if (_isUpgrading) return;
            if (TowerLevel >= 3) return;
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
            await UniTask.Delay(TimeSpan.FromSeconds(towerManager.towerLevels[TowerLevel].constructionTime),
                cancellationToken: _cts.Token);
            LevelUpEnd();
            range = towerManager.towerLevels[TowerLevel].attackRange;
        }

        private async UniTaskVoid UniqueUpgradeAsync()
        {
            TowerLevel += 2;
            _meshFilter.mesh = towerManager.towerLevels[TowerLevel].consMesh;
            _isUpgrading = true;
            await UniTask.Delay(TimeSpan.FromSeconds(towerManager.towerLevels[TowerLevel].constructionTime),
                cancellationToken: _cts.Token);
            _meshFilter.mesh = towerManager.towerLevels[TowerLevel].towerMesh;
        }

        protected virtual void LevelUpStart()
        {
            TowerLevel++;
            _meshFilter.mesh = towerManager.towerLevels[TowerLevel].consMesh;
            _isUpgrading = true;
        }

        protected virtual void LevelUpEnd()
        {
            _meshFilter.mesh = towerManager.towerLevels[TowerLevel].towerMesh;
            _isUpgrading = false;
        }

        private void OpenEditPanel()
        {
            _editCanvasController.OpenEditPanel(_cam.WorldToScreenPoint(transform.position),
                towerManager.towerLevels[TowerLevel].towerInfo,
                towerManager.towerLevels[TowerLevel].towerName);
        }
        //==================================Custom function====================================================
    }
}