using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace UnitControl
{
    public class BarracksUnit : Unit
    {
        private NavMeshAgent _nav;
        private SphereCollider _sphereCollider;

        public event Action OnDeadEvent;

        private void Awake()
        {
            _nav = GetComponent<NavMeshAgent>();
            _sphereCollider = GetComponentInChildren<SphereCollider>();
        }

        private void Update()
        {
            if (!IsTargeting) return;
            _nav.SetDestination(Target.position);
            if (_nav.remainingDistance <= _nav.stoppingDistance)
            {
                if (!attackAble) return;
                Attack();
                StartCoolDown().Forget();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnDeadEvent?.Invoke();
            OnDeadEvent = null;
        }

        protected override void Attack()
        {
            AutoAttack().Forget();
        }

        private async UniTaskVoid AutoAttack()
        {
            _sphereCollider.enabled = true;
            await UniTask.Delay(100, cancellationToken: cts.Token);
            _sphereCollider.enabled = false;
        }
    }
}