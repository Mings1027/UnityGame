using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace GameControl
{
    public class Health : MonoBehaviour
    {
        private Renderer _renderer;
        private Sequence _hitEffectSequence;
        private Tween _destroyTween;

        public bool IsDead { get; private set; }

        [SerializeField] private int curHealth, maxHealth;
        [SerializeField] private UnityEvent<GameObject> hitWithReference;
        [SerializeField] private UnityEvent<GameObject> deathWithReference;

        [SerializeField] private float disappearTime;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _renderer.material.color = Color.white;

            _hitEffectSequence = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(_renderer.material.DOColor(Color.red, 0f))
                .Append(_renderer.material.DOColor(Color.white, 1f))
                .Pause();
            _destroyTween = DOVirtual.DelayedCall(disappearTime, () => gameObject.SetActive(false))
                .SetAutoKill(false).Pause();
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
            _hitEffectSequence.Restart();
            if (curHealth > 0)
            {
                hitWithReference?.Invoke(sender);
            }
            else
            {
                deathWithReference?.Invoke(sender);
                IsDead = true;
                _destroyTween.Restart();
            }
        }
    }
}