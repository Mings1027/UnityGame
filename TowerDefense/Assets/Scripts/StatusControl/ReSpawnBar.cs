using System;
using DG.Tweening;

namespace StatusControl
{
    public class ReSpawnBar : StatusBar
    {
        private Sequence _loadingSequence;
        public event Action OnRespawnEvent; 
        private void OnDisable()
        {
            _loadingSequence?.Kill();
            OnRespawnEvent = null;
        }

        public void Init()
        {
            _loadingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(transform.DOScale(transform.localScale.x, 0.5f).From(0).SetEase(Ease.OutBack));
        }

        public void StartLoading(float loadingDelay)
        {
            _loadingSequence.Restart();
            slider.DOValue(1, loadingDelay).From(0).OnComplete(() =>
            {
                _loadingSequence.OnRewind(() => gameObject.SetActive(false)).PlayBackwards();
                OnRespawnEvent?.Invoke();
            }).Restart();
        }

        public void StopLoading()
        {
            DOTween.Sequence().Append(transform.DOScale(0, 0.5f)).SetEase(Ease.OutBack)
                .Join(transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack))
                .OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}