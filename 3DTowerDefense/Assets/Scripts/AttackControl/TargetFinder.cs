using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour
    {
        private Vector3 _checkRangePoint;
        private float _atkDelay;
        private CancellationTokenSource _cts;
        private Transform _target;
        private Vector3 _centerPos;
        private Collider[] _targetColliders;
        private int _minDamage, _maxDamage;

        public float AtkRange { get; private set; }

        public int Damage => Random.Range(_minDamage, _maxDamage);

        public bool attackAble;
        public int colliderSize;

        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask targetLayer;

        private void Awake()
        {
            _targetColliders = new Collider[colliderSize];
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            attackAble = true;
        }

        private void OnDisable()
        {
            _cts.Cancel();
            CancelInvoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_centerPos, AtkRange);
        }

        public void SetUp(float attackDelay, float attackRange, int unitMinDamage, int unitMaxDamage,
            int unitHealth = 0)
        {
            _atkDelay = attackDelay;
            _minDamage = unitMinDamage;
            _maxDamage = unitMaxDamage;
            AtkRange = attackRange;
            if (TryGetComponent(out Health h)) h.InitializeHealth(unitHealth);
        }

        public async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            attackAble = true;
        }

        public (Transform, bool) FindClosestTarget()
        {
            Physics.Raycast(transform.position, Vector3.down, out var hit, 100, groundLayer);
            _centerPos = hit.point;

            var size = Physics.OverlapSphereNonAlloc(_centerPos, AtkRange, _targetColliders, targetLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            switch (size)
            {
                case <= 0:
                    return (null, false);
                case 1:
                    return (_targetColliders[0].transform, true);
            }

            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _targetColliders[i].transform;
            }

            return (nearestEnemy, nearestEnemy != null);
        }
    }
}