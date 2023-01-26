using System;
using System.Threading;
using BulletControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace PlayerAttack
{
    public class AutoAttack : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer weaponSprite;
        [SerializeField] private Transform gunTransform, shootPoint;
        [SerializeField] private float delay;
        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        public Collider2D foundEnemy;
        private bool _isAttack;

        private void OnEnable()
        {
            _isAttack = false;
        }

        private void Start()
        {
            InvokeRepeating(nameof(SearchNearestEnemy), 0, 0.5f);
        }

        private void Update()
        {
            Targeting();
        }

        private void SearchNearestEnemy()
        {
            foundEnemy = Physics2D.OverlapCircle(transform.position, range, enemyLayer);
        }


        public void Attack(CancellationTokenSource cts)
        {
            if (!foundEnemy) return;
            if (_isAttack) return;
            Auto(cts).Forget();
        }

        private async UniTaskVoid Auto(CancellationTokenSource cts)
        {
            _isAttack = true;
            StackObjectPool.Get<BulletRange>("RangeBullet", shootPoint.position,
                gunTransform.rotation * Quaternion.Euler(0, 0, -90));
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cts.Token);
            _isAttack = false;
        }

        public void Flip()
        {
            if (!foundEnemy) return;
            weaponSprite.flipY = foundEnemy.transform.position.x < transform.position.x;
        }

        private void Targeting()
        {
            if (!foundEnemy) return;
            var rotation = foundEnemy.transform.position - transform.position;
            var rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
            gunTransform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}