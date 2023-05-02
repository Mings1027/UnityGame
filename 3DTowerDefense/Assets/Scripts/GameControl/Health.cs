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
        private Renderer _renderer;

        public bool IsDead { get; private set; }

        [SerializeField] private int curHealth, maxHealth;
        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;

        [SerializeField] private float disappearTime;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
        }

        private void OnEnable()
        {
            Init(maxHealth);
            _renderer.material.color = Color.white;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        public void Init(int healthValue)
        {
            curHealth = healthValue;
            maxHealth = healthValue;
            IsDead = false;
        }

        public void TakeDamage(int amount, GameObject sender)
        {
            if (IsDead) return;
            curHealth -= amount;
            HitEffectTask().Forget();
            if (curHealth > 0)
            {
                hitWithReference?.Invoke(sender);
            }
            else
            {
                deathWithReference?.Invoke(sender);
                IsDead = true;
                DeadTask().Forget();
            }
        }

        private async UniTaskVoid HitEffectTask()
        {
            _renderer.material.color = Color.red;
            await UniTask.Delay(300, cancellationToken: _cts.Token);
            _renderer.material.color = Color.white;
        }

        private async UniTaskVoid DeadTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
            gameObject.SetActive(false);
        }
    }
}