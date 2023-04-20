using System;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MeleeWeapon : MonoBehaviour
    {
        private AudioSource audioSource;
        private Collider[] _attackCollier;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform hitBox;
        [SerializeField] private float hitBoxSize;
        [SerializeField] private AudioClip enableAudio;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            _attackCollier = new Collider[1];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hitBox.position, hitBoxSize);
        }

        public void Attack(Transform target, int damage)
        {
            audioSource.PlayOneShot(enableAudio);
            var size = Physics.OverlapSphereNonAlloc(hitBox.position, hitBoxSize, _attackCollier, enemyLayer);
            if (size <= 0) return;

            if (target.TryGetComponent(out Health h))
            {
                StackObjectPool.Get("SwordEffect", hitBox.position);
                h.TakeDamage(damage, target.gameObject);
            }
        }
    }
}