using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class WeaponParent : MonoBehaviour
    {
        private Animator anim;

        [SerializeField] private Transform circleOrigin;
        [SerializeField] private float radius;
        private static readonly int IsAttack = Animator.StringToHash("isAttack");

        private void Awake()
        {
            anim = GetComponentInChildren<Animator>();
        }

        public void Attack()
        {
            anim.SetTrigger(IsAttack);
        }

        public void DetectCollidors()
        {
            //start when call Attack()
            Func<Vector2, float, Collider2D[]> overlapCircleAll = Physics2D.OverlapCircleAll;
            foreach (var enemy in overlapCircleAll(circleOrigin.position, radius))
            {
                if (!enemy.gameObject.CompareTag("Enemy")) return;
                var value = Random.Range(1, 6);
                enemy.GetComponent<KnockBack>().thrust = value;
                enemy.GetComponent<Health>().GetHit(value, transform.parent.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var pos = circleOrigin == null ? Vector3.zero : circleOrigin.position;
            Gizmos.DrawWireSphere(pos, radius);
        }
    }
}