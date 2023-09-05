using UnityEngine;

namespace ProjectileControl
{
    public class FollowProjectile : MonoBehaviour
    {
        private bool isTargeting;

        public Transform target { get; set; }

        private void OnEnable()
        {
            isTargeting = true;
        }

        private void FixedUpdate()
        {
            if (!isTargeting) return;
            if (!target.gameObject.activeSelf)
            {
                isTargeting = false;
                return;
            }

            transform.position = target.position + Random.insideUnitSphere * 0.1f;
        }

        private void OnDisable()
        {
            target = null;
            isTargeting = false;
        }
    }
}