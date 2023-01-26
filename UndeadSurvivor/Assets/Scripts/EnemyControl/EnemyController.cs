using UnityEngine;

namespace EnemyControl
{
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyController : MonoBehaviour
    {
        private EnemyAI _enemyAI;

        private void Awake()
        {
            _enemyAI = GetComponent<EnemyAI>();
        }

        private void FixedUpdate()
        {
            _enemyAI.EnemyMove();
        }

        private void LateUpdate()
        {
            _enemyAI.FlipSprite();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.collider.CompareTag("Player")) return;
            _enemyAI.EnemyDamaged( col);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Area")) return;
            _enemyAI.EnemyReLocation();
        }

        
    }
}