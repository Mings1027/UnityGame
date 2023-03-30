using GameControl;
using UnityEngine;

namespace WeaponControl
{
    public class MeleeWeapon : MonoBehaviour
    {
        private Collider[] _attackCollier;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform hitBox;
        [SerializeField] private float hitBoxSize;

        private void Awake()
        {
            _attackCollier = new Collider[1];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hitBox.position, hitBoxSize);
        }

        public void Attack(Transform target, int damage)
        {
            var size = Physics.OverlapSphereNonAlloc(hitBox.position, hitBoxSize, _attackCollier, enemyLayer);
            if (size <= 0) return;

            for (var i = 0; i < size; i++)
            {
                if (target.TryGetComponent(out Health h))
                    h.GetHit(damage, target.gameObject);
            }
        }
    }
}