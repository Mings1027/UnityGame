using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using WeaponControl;

namespace PlayerControl
{
    public class PlayerAutoAttack : MonoBehaviour
    {
        [SerializeField] private float range;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Collider2D foundEnemy;

        [SerializeField] private SpriteRenderer weaponSprite, playerSprite;
        [SerializeField] private Transform shootPoint, gunTransform;
        [SerializeField] private float atkDelay;
        private bool _isAttack;
        private CancellationTokenSource _cts;

        private void Start()
        {
            InvokeRepeating(nameof(SearchNearestEnemy), 0, 0.5f);
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts.Cancel();
        }

        private void Update()
        {
            if (!foundEnemy) return;
            Targeting();
            RangeAttack().Forget();
        }

        private void LateUpdate()
        {
            if (!foundEnemy) return;
            FlipSprite();
        }

        private void Targeting()
        {
            var rotation = foundEnemy.transform.position - transform.position;
            var rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
            gunTransform.rotation = Quaternion.Euler(0, 0, rotZ);
        }

        private async UniTaskVoid RangeAttack()
        {
            if (_isAttack) return;
            _isAttack = true;
            StackObjectPool.Get<RangeBullet>("Bullet", shootPoint.position,
                gunTransform.rotation * Quaternion.Euler(0, 0, -90));
            await UniTask.Delay(TimeSpan.FromSeconds(atkDelay), cancellationToken: _cts.Token);
            _isAttack = false;
        }

        private void FlipSprite()
        {
            weaponSprite.flipY = playerSprite.flipX = foundEnemy.transform.position.x < transform.position.x;
        }


        private void SearchNearestEnemy()
        {
            foundEnemy = Physics2D.OverlapCircle(transform.position, range, enemyLayer);
        }


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}