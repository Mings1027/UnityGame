using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameControl
{
    public class UnitHealth : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private HealthBar healthBar;

        public bool IsDead { get; private set; }

        [SerializeField] private float curHealth, maxHealth;
        [SerializeField] private float disappearTime;

        private void Awake()
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            curHealth = maxHealth;
            IsDead = false;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        public void Init(float healthValue)
        {
            curHealth = healthValue;
            maxHealth = healthValue;
            IsDead = false;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            curHealth -= damage;
            healthBar.UpdateHealthBar(curHealth / maxHealth);

            if (curHealth > 0) return;
            //죽을때 해골 이펙트 띄워도 괜찮을듯
            IsDead = true;
            DeadTask().Forget();
        }

        private async UniTaskVoid DeadTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
            gameObject.SetActive(false);
        }
    }
}