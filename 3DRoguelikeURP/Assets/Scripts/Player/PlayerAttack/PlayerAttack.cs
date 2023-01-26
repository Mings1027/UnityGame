using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace Player.PlayerAttack
{
    public class PlayerAttack : MonoBehaviour
    {
        private ObjectPool<PlayerBullet> bulletPool;
        [SerializeField] private PlayerBullet bulletPrefab;
        [SerializeField] private float atkDelay;
        [SerializeField] private bool isAttack;
        [SerializeField] private int bulletPoolMaxSize;

        private void Start()
        {
            bulletPool = new ObjectPool<PlayerBullet>(
                createFunc: () =>
                {
                    var createBullet = Instantiate(bulletPrefab, transform);
                    createBullet.SetPool(bulletPool);
                    return createBullet;
                },
                actionOnGet: bullet =>
                {
                    bullet.gameObject.SetActive(true);
                    bullet.Reset();
                },
                actionOnRelease: bullet => { bullet.gameObject.SetActive(false); },
                actionOnDestroy: bullet => { Destroy(bullet.gameObject); }, maxSize: bulletPoolMaxSize);
        }

        public void Attack()
        {
            if (isAttack) return;
            isAttack = true;
            bulletPool.Get().rigid.transform.position = transform.position;
            DOVirtual.DelayedCall(atkDelay, () => { isAttack = false; });
        }
    }
}