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
        private Transform _target;
        private bool _isTargeting;
        private int _towerLevel;
        private CancellationTokenSource _cts;

        [SerializeField] private TowerLevelManager towerLevelManager;
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
            targets = new Collider[5];
        }

        //==================================Event Method=====================================================
        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
            UpgradeTower();
        }

        private void OnDisable()
        {
            _cts.Cancel();
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            _towerLevel = 0;
            _meshFilter.mesh = towerLevelManager.towerLevels[0].towerMesh;
        }

        private void Update()
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

        //==================================Event Method=====================================================
        //==================================Custom Method====================================================
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
                _target = nearestEnemy;
                _isTargeting = true;
            }
            else
            {
                _target = null;
                _isTargeting = false;
            }
        }

        public void UpgradeTower()
        {
            if (_towerLevel >= towerLevelManager.towerLevels.Length - 1) return;
            Upgrade().Forget();
        }

        private async UniTaskVoid Upgrade()
        {
            _cts.Cancel();
            _towerLevel++;
            _meshFilter.mesh = towerLevelManager.towerLevels[_towerLevel].consMesh;
            await UniTask.Delay(TimeSpan.FromSeconds(towerLevelManager.towerLevels[_towerLevel].constructionTime));
            _meshFilter.mesh = towerLevelManager.towerLevels[_towerLevel].towerMesh;
        }

        private void OpenTowerInfo()
        {
            _towerInfoSystem.OpenInfo(infoPanelTransform.position,
                towerLevelManager.towerLevels[_towerLevel].towerInfo,
                towerLevelManager.towerLevels[_towerLevel].towerName);
        }
        //==================================Custom Method====================================================
    }
}