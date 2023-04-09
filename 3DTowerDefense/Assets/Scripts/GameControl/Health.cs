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
        private CancellationTokenSource cts;
        private Renderer _renderer;

        public bool IsDead { get; private set; }

        [SerializeField] private int curHealth, maxHealth;
        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;

        [SerializeField] private float disappearTime;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _renderer.material.color = Color.white;
        }

        private void OnEnable()
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            cts?.Cancel();
        }

        public void Init(int healthValue)
        {
            curHealth = healthValue;
            maxHealth = healthValue;
            IsDead = false;
        }

        public void GetHit(int amount, GameObject sender)
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
            await UniTask.Delay(1000, cancellationToken: cts.Token);
            _renderer.material.color = Color.white;
        }

        private async UniTaskVoid DeadTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: cts.Token);
            gameObject.SetActive(false);
        }
    }
}