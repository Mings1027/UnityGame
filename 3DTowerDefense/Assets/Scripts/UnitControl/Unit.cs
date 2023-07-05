using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnitControl
{
    [DisallowMultipleComponent]
    public abstract class Unit : MonoBehaviour
    {
        private float _atkDelay;
        private int _minDamage, _maxDamage;

        protected Animator anim;
        protected Rigidbody rigid;
        protected CancellationTokenSource cts;
        
        protected bool isCoolingDown;
        protected int Damage => Random.Range(_minDamage, _maxDamage);

        protected virtual void Awake()
        {
            anim = GetComponent<Animator>();
            rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            isCoolingDown = true;
        }

        protected virtual void OnDisable()
        {
            cts?.Cancel();
        }

        protected abstract void Attack();

        protected async UniTaskVoid StartCoolDown()
        {
            isCoolingDown = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_atkDelay), cancellationToken: cts.Token);
            isCoolingDown = true;
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