using System;
using System.Threading;
using AttackControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    public abstract class Unit : MonoBehaviour
    {
        private float atkDelay;
        private int _minDamage, _maxDamage;
        private CancellationTokenSource _cts;

        protected bool attackAble;
        protected NavMeshAgent nav;
        protected int Damage => Random.Range(_minDamage, _maxDamage);

        public bool IsTargeting { get; set; }
        public Transform Target { get; set; }

        [SerializeField] [Range(0, 1)] private float smoothTurnSpeed;

        protected virtual void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
        }

        protected virtual void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Target = null;
            IsTargeting = false;
            attackAble = true;
            InvokeRepeating(nameof(CheckTarget), 1, 1);
        }

        protected abstract void Update();

        private void LateUpdate()
        {
            if (!IsTargeting) return;
            LookTarget();
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
        }

        private void CheckTarget()
        {
            if (Target == null) return;
            if (Target.gameObject.activeSelf) return;
            Target = null;
            IsTargeting = false;
        }

        protected abstract void Attack();

        protected async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: _cts.Token);
            attackAble = true;
        }

        public void Init(int minD, int maxD, float delay, int health)
        {
            _minDamage = minD;
            _maxDamage = maxD;
            atkDelay = delay;
            GetComponent<Health>().Init(health);
        }

        private void LookTarget()
        {
            var direction = Target.position + Target.forward;
            var dir = direction - transform.position;
            var yRot = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            var lookRot = Quaternion.Euler(0, yRot, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, smoothTurnSpeed);
        }
    }
}