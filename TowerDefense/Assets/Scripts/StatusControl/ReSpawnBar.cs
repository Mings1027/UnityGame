using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace StatusControl
{
    public class ReSpawnBar : ProgressBar
    {
        private Camera _cam;
        private Vector3 _position;
        private CancellationTokenSource _cts;
        private Sequence _loadingSequence;

        protected override void Awake()
        {
            base.Awake();
            _cam = Camera.main;
        }

        private void OnDisable()
        {
            _loadingSequence?.Kill();
        }

        public void Init()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            transform.position = _cam.WorldToScreenPoint(_position);
            _loadingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(transform.localScale.x, 0.5f).From(0).SetEase(Ease.OutBack))
                .Join(transform.DOLocalMoveY(3, 0.5f).SetEase(Ease.OutBack));
        }

        public async UniTask StartLoading(float loadingDelay)
        {
            _loadingSequence.Restart();
            await slider.DOValue(1, loadingDelay).From(0).WithCancellation(_cts.Token);
            _loadingSequence.PlayBackwards();
        }

        public async UniTaskVoid StopLoading()
        {
            _cts?.Cancel();
            _loadingSequence.PlayBackwards();
            while (_loadingSequence.IsComplete())
            {
                await UniTask.Yield();
                gameObject.SetActive(false);
            }
        }
    }
}