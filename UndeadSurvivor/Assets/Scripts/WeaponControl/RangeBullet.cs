using EnemyControl;
using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class RangeBullet : MonoBehaviour
    {
        [SerializeField] private int bulletSpeed;
        [SerializeField] private int damage;

        private Rigidbody2D _rigid;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rigid.velocity = transform.up * bulletSpeed;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            col.GetComponent<EnemyHealth>().GetHit(damage, GameManager.Instance.player.gameObject);
            gameObject.SetActive(false);
        }
    }
}