using DG.Tweening;
using GameControl;
using UnityEngine;

namespace Player.PlayerAttack
{
    public class HitEffect : MonoBehaviour
    {
        private new ParticleSystem particleSystem;
        [SerializeField] private float lifeTime;

        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            particleSystem.Play();
            Invoke(nameof(DestroyBullet), lifeTime);
            // DOVirtual.DelayedCall(lifeTime, () => gameObject.SetActive(false));
        }

        private void OnDisable()
        {
            // ObjectPooling.ReturnToPool(gameObject);
        }

        private void DestroyBullet() => gameObject.SetActive(false);

        // private void GameObjectActive() => gameObject.SetActive(false);
    }
}