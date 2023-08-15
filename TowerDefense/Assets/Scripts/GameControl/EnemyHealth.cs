using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameControl
{
    public class EnemyHealth : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        private HealthBar healthBar;
        private float curHealth, maxHealth;

        [SerializeField] private float disappearTime;

        public event Action OnDeadEvent;
        public event Action OnIncreaseCoinEvent;
        public event Action OnDecreaseLifeCountEvent;

        private void Awake()
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            curHealth = maxHealth;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            OnDeadEvent?.Invoke();

            if (curHealth > 0)
            {
                OnDecreaseLifeCountEvent?.Invoke();
            }
            else
            {
                OnIncreaseCoinEvent?.Invoke();
            }

            OnDeadEvent = null;
            OnDecreaseLifeCountEvent = null;
            OnIncreaseCoinEvent = null;
        }

        public void Init(float healthValue)
        {
            curHealth = healthValue;
            maxHealth = healthValue;
        }

        public void TakeDamage(float damage)
        {
            curHealth -= damage;
            healthBar.UpdateHealthBar(curHealth / maxHealth);
            if (curHealth > 0) return;
            DeadTask().Forget();
        }

        private async UniTaskVoid DeadTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
            gameObject.SetActive(false);
        }
    }
}