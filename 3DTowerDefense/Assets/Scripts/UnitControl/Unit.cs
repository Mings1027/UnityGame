using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public abstract class Unit : MonoBehaviour
    {
        private float _atkDelay;
        private int _minDamage, _maxDamage;

        private CancellationTokenSource _cts;

        protected NavMeshAgent nav;
        protected bool attackAble;
        protected int Damage => Random.Range(_minDamage, _maxDamage);

        protected virtual void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
        }

        protected virtual void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            attackAble = true;
        }

        protected virtual void OnDisable()
        {
            _cts?.Cancel();
            CancelInvoke();
        }

        protected abstract void Attack();

        protected async UniTaskVoid StartCoolDown()
        {
            attackAble = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: _cts.Token);
            attackAble = true;
        }

        public void Init(int minD, int maxD, float delay, float health)
        {
            _minDamage = minD;
            _maxDamage = maxD;
            _atkDelay = delay;
            GetComponent<Health>().Init(health);
        }
    }
}