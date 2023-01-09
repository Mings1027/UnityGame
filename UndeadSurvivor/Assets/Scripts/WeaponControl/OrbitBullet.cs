using EnemyControl;
using UnityEngine;

namespace WeaponControl
{
    public class OrbitBullet : MonoBehaviour
    {
        [SerializeField] private int damage;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            col.GetComponent<EnemyHealth>().GetHit(damage, transform.parent.gameObject);
        }
    }
}