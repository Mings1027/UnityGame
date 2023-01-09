using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace PlayerControl
{
    public class PlayerMeleeAttack : MonoBehaviour
    {
        private bool _isAttack;
        [SerializeField] private Transform meleePoint;
        [SerializeField] private float delay;
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _isAttack = false;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        private void Update()
        {
            Attack().Forget();
        }

        private async UniTaskVoid Attack()
        {
            if (_isAttack) return;
            _isAttack = true;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _cts.Token);
            StackObjectPool.Get("MeleeBullet", meleePoint.position, transform.rotation);
            _isAttack = false;
        }
    }
}