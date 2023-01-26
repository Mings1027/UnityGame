using GameControl;
using PlayerControl;
using UnityEngine;

namespace BulletControl
{
    public class BulletRange : MonoBehaviour
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
            col.GetComponent<Health>().GetHit(damage, PlayerController.Rigid.gameObject);
            gameObject.SetActive(false);
        }
    }
}