using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour
    {
        private CancellationTokenSource cts;
        
        private GameObject targetObj;
        private Collider[] _targetColliders;
        private float _atkDelay;
        private int _minDamage, _maxDamage;
        private float atkRange;

        public int Damage => Random.Range(_minDamage, _maxDamage);
        public bool attackAble;

        public Transform Target { get; private set; }
        public bool IsTargeting { get; private set; }

        [SerializeField] private bool lookObject;
        [SerializeField] private float smoothTurnSpeed;
        [SerializeField] private LayerMask targetLayer;

        private void Awake()
        {
            _targetColliders = new Collider[5];
            atkRange = 5;
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            attackAble = true;
            InvokeRepeating(nameof(ClosestTarget), 0, 1f);
        }

        private void LateUpdate()
        {
            if (!IsTargeting || !lookObject) return;
            LookTarget();
        }

        private void OnDisable()
        {
            cts?.Dispose();
            CancelInvoke();
            Target = null;
            IsTargeting = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }

        private void LookTarget()
        {
            var direction = Target.position + Target.forward;
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var lookRot = Quaternion.Euler(0, yRot, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothTurnSpeed);
        }

        public void SetUp(int unitMinDamage, int unitMaxDamage, float attackRange, float attackDelay)
        {
            _minDamage = unitMinDamage;
            _maxDamage = unitMaxDamage;
            atkRange = attackRange;
            _atkDelay = attackDelay;
        }

        public async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            attackAble = true;
        }

        private void ClosestTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(transform.position, atkRange, _targetColliders, targetLayer);
            if (size <= 0)
            {
                Target = null;
                IsTargeting = false;
                return;
            }

            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            for (var i = 0; i < size; i++)
            {
                if (_targetColliders[i].gameObject == targetObj) continue;

                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _targetColliders[i].transform;
            }

            Target = nearestEnemy;
            IsTargeting = nearestEnemy != null;
            targetObj = nearestEnemy != null ? nearestEnemy.gameObject : null;
        }
    }
}