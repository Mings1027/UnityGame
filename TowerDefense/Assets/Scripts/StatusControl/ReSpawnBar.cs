using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace StatusControl
{
    public class ReSpawnBar : ProgressBar
    {
        private CancellationTokenSource _cts;
        private Sequence _loadingSequence;

        private void OnDisable()
        {
            _loadingSequence?.Kill();
        }

        public void Init()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _loadingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(transform.localScale.x, 0.5f).From(0).SetEase(Ease.OutBack));
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
            await DOTween.Sequence().Append(transform.DOScale(0, 0.5f)).SetEase(Ease.OutBack)
                .Join(transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack));
            gameObject.SetActive(false);
        }
    }
}