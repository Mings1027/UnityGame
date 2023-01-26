using GameControl;
using UnityEngine;

namespace BulletControl
{
    public class BulletOrbit : MonoBehaviour
    {
        [SerializeField] private int damage;
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            col.GetComponent<Health>().GetHit(damage, transform.parent.gameObject);
        }
    }
}