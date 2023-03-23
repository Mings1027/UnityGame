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
        private Renderer _renderer;
        private CancellationTokenSource _cts;

        public int CurHealth => curHealth;
        [SerializeField] private int curHealth, maxHealth;

        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;
        [SerializeField] private bool isDead;
        [SerializeField] private float disappearTime;

        private void OnEnable()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _renderer.material.color = Color.white;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            curHealth = maxHealth;
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

        public void GetHit(int amount, GameObject sender)
        {
            if (isDead) return;
            curHealth -= amount;
            HitEffect();
            if (curHealth > 0)
            {
                hitWithReference?.Invoke(sender);
            }
            else
            {
                deathWithReference?.Invoke(sender);
                isDead = true;
                DOVirtual.DelayedCall(disappearTime, () => gameObject.SetActive(false));
                // await UniTask.Delay(TimeSpan.FromSeconds(disappearTime), cancellationToken: _cts.Token);
                // gameObject.SetActive(false);
            }
        }

        private void HitEffect()
        {
            _renderer.material.color = Color.red;
            DOVirtual.DelayedCall(0.5f,()=>_renderer.material.color = Color.white);
            // await UniTask.Delay(500, cancellationToken: _cts.Token);
            // _renderer.material.color = Color.white;
        }
    }
}