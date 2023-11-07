using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace StatusControl
{
    public class ReSpawnBar : ProgressBar
    {
        private CancellationTokenSource _cts;

        protected override void OnEnable()
        {
            base.OnEnable();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTask UpdateBarEvent()
        {
            await slider.DOValue(1, 5f).From(0).WithCancellation(_cts.Token);
            enabled = false;
        }

        public void StopReSpawning() => _cts?.Cancel();
    }
}