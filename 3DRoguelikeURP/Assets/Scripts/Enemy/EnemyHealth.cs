using UnityEngine;
using UnityEngine.Events;

namespace Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth, curHealth;
        [SerializeField] private UnityEvent<Transform> onHitEvent, onDeathEvent;

        private bool isDead;

        private void OnEnable()
        {
            curHealth = maxHealth;
            isDead = false;
        }

        public void OnDamage(int damage, Transform sender)
        {
            if (isDead) return;
            curHealth -= damage;
            if (curHealth > 0)
            {
                onHitEvent?.Invoke(sender);
            }
            else
            {
                isDead = true;
                onDeathEvent?.Invoke(sender);
                gameObject.SetActive(false);
            }
        }
    }
}