using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace GameControl
{
    public class Health : MonoBehaviour
    {
        private CancellationTokenSource _cts;

        public bool IsDead { get; private set; }
        public float CurHealth => curHealth;

        [SerializeField] private HealthBar healthBar;
        [SerializeField] private float curHealth, maxHealth;
        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;

        [SerializeField] private float disappearTime;

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
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

        public void TakeDamage(float damage, GameObject sender)
        {
            if (IsDead) return;
            curHealth -= damage;
            healthBar.UpdateHealthBar(curHealth / maxHealth);
            if (curHealth > 0)
            {
                hitWithReference?.Invoke(sender);
            }
            else
            {
                //죽을때 해골 이펙트 띄워도 괜찮을듯
                deathWithReference?.Invoke(sender);
                IsDead = true;
                DeadTask().Forget();
            }
        }


        private async UniTaskVoid DeadTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
            gameObject.SetActive(false);
        }
    }
}