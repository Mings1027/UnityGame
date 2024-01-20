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

        public ushort ManaRegenValue { get; set; }

        [SerializeField] private TMP_Text manaText;

        protected override void OnEnable()
        {
            base.OnEnable();
            ManaRegenValue = 1;
        }

        public override void Init(float amount)
        {
            base.Init(amount);
            manaText.text = Current + " / " + Initial;
        }

        public void Damage(float amount)
        {
            Current -= amount;
            manaText.text = Current + " / " + Initial;
        }

        public void Heal(in ushort amount)
        {
            if (IsFull) return;
            Current += amount;
            if (Current > Initial) Current = Initial;
            manaText.text = Current + " / " + Initial;
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