using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour
    {
        private Vector3 _checkRangePoint;
        private Collider[] _results;
        private float _atkDelay, _atkRange;
        private CancellationTokenSource _cts;
        private Transform _target;

        public int Damage { get; private set; }
        public bool attackAble;
        [SerializeField] private LayerMask targetLayer;
        
        private void Awake()
        {
            _results = new Collider[3];
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

        // private void Update()
        // {
        //     if (!_attackAble || !_isTargeting) return;
        //     Attack();
        //     StartCoolDown().Forget();
        // }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _atkRange);
        }

        public void SetUp(float attackDelay, float attackRange, int unitDamage, int unitHealth = 0)
        {
            _atkDelay = attackDelay;
            _atkRange = attackRange;
            Damage = unitDamage;
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
            var size = Physics.OverlapSphereNonAlloc(transform.position, _atkRange, _results, targetLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            if (size <= 0) return (null, false);
            for (var i = 0; i < size; i++)
            {
                var distanceToResult = Vector3.SqrMagnitude(transform.position - _results[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _results[i].transform;
            }

            return (nearestEnemy, true);
        }
    }
}