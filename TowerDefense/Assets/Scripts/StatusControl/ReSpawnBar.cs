using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace StatusControl
{
    public class ReSpawnBar : ProgressBar
    {
        private CancellationTokenSource cts;

        protected override void OnEnable()
        {
            base.OnEnable();
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            cts?.Cancel();
            cts?.Dispose();
        }

        public async UniTask UpdateBarEvent()
        {
            await slider.DOValue(1, 5f).From(0).WithCancellation(cts.Token);
            enabled = false;
        }

        public void StopReSpawning() => cts?.Cancel();
    }
}