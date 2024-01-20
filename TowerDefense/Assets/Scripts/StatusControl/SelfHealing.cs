using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace StatusControl
{
    public class SelfHealing : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private Health _health;
        [SerializeField] private ushort healAmount;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            Healing().Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid Healing()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
                _health.Heal(healAmount);
            }
        }
    }
}