using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;

namespace PlayerAttack
{
    public class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Transform meleePoint;

        [SerializeField] private float delay;
        private bool _isAttack;

        private CancellationTokenSource _cts;
        private Sequence _meleeTween;

        private void Start()
        {
            _meleeTween = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(DOVirtual.DelayedCall(delay,
                    () => { StackObjectPool.Get("MeleeBullet", meleePoint.position, transform.rotation); }))
                .SetLoops(5);
        }

        private void OnEnable()
        {
            _meleeTween.Restart();

            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        private async UniTaskVoid Melee()
        {
            if (_isAttack) return;
            _isAttack = true;
            StackObjectPool.Get("MeleeBullet", meleePoint.position, transform.rotation);
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _cts.Token);
            _isAttack = false;
        }

        public void Flip()
        {
            var rot = transform.rotation;
            rot.z = playerSprite.flipX ? 180 : 0;
            transform.rotation = rot;
        }
    }
}