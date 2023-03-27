using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AttackControl
{
    public class TargetFinder : MonoBehaviour
    {
        private Collider[] _targetColliders;
        private float _atkDelay;
        private int _minDamage, _maxDamage;
        private CancellationTokenSource cts;

        public float AtkRange { get; private set; }
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
            AtkRange = 5;
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
            Gizmos.DrawWireSphere(transform.position, AtkRange);
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
            AtkRange = attackRange;
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
            var size = Physics.OverlapSphereNonAlloc(transform.position, AtkRange, _targetColliders, targetLayer);
            var shortestDistance = Mathf.Infinity;
            Transform nearestEnemy = null;

            for (var i = 0; i < size; i++)
            {
                var distanceToResult =
                    Vector3.SqrMagnitude(transform.position - _targetColliders[i].transform.position);
                if (distanceToResult >= shortestDistance) continue;
                shortestDistance = distanceToResult;
                nearestEnemy = _targetColliders[i].transform;
            }

            Target = size <= 0 ? null : nearestEnemy;
            IsTargeting = Target != null;
        }
    }
}