using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GameControl
{
    public class Health : MonoBehaviour
    {
        private CancellationTokenSource _cts;
        [SerializeField] private int curHealth, maxHealth;

        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;
        [SerializeField] private bool isDead;
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

        public void InitializeHealth(int healthValue)
        {
            curHealth = healthValue;
            maxHealth = healthValue;
            isDead = false;
        }

        public async UniTaskVoid GetHit(int amount, GameObject sender)
        {
            if (isDead) return;
            curHealth -= amount;
            if (curHealth > 0)
            {
                hitWithReference?.Invoke(sender);
            }
            else
            {
                deathWithReference?.Invoke(sender);
                isDead = true;
                await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
                gameObject.SetActive(false);
            }
        }
    }
}