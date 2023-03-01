using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameControl
{
    [Serializable]
    public class UniTaskCooldown
    {
        public float delay;
        public CancellationTokenSource cts;
        public bool IsCoolingDown { get; private set; }

        public async UniTaskVoid StartCooldown()
        {
            IsCoolingDown = true;
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);
            IsCoolingDown = false;
        }
    }
}