using System.Threading;
using Cysharp.Threading.Tasks;
using InterfaceControl;
using TMPro;
using UnityEngine;

namespace StatusControl
{
    public class Mana : Progressive, IDamageable, IHealable
    {
        private CancellationTokenSource _cts;

        public float ManaRegenValue { get; set; }

        [SerializeField] private TMP_Text manaText;

        protected override void OnEnable()
        {
            base.OnEnable();
            ManaRegenValue = 1;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_cts == null) return;
            if (_cts.IsCancellationRequested) return;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public override void Init(float amount)
        {
            base.Init(amount);
            manaText.text = Current + " / " + Initial;
        }

        public void Damage(float amount)
        {
            if (Current <= 0) return;
            Current -= amount;
            manaText.text = Current + " / " + Initial;
            if (Current <= 0) Current = 0;
        }

        public void Heal(in float amount)
        {
            if (IsFull) return;
            Current += amount;
            manaText.text = Current + " / " + Initial;
            if (Current > Initial) Current = Initial;
        }

        private async UniTaskVoid AutoManaRegenerate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(1000, cancellationToken: _cts.Token);
                Heal(ManaRegenValue);
            }
        }

        public void StartManaRegen()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            AutoManaRegenerate().Forget();
        }

        public void StopManaRegen()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}