using GameControl;
using PlayerControl;
using UnityEngine;

namespace EnemyControl
{
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyController : MonoBehaviour
    {
        private EnemyAI _enemyAI;
        private Animator _anim;
        private EnemyHealth _enemyHealth;
        private float _speed;
        private int _damage;
        [SerializeField] private RuntimeAnimatorController[] animController;

        private void Awake()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _anim = GetComponent<Animator>();
            _enemyHealth = GetComponent<EnemyHealth>();
        }

        public void Init(SpawnData data)
        {
            _anim.runtimeAnimatorController = animController[data.spriteType];
            _speed = data.speed;
            _damage = data.damage;
            _enemyHealth.InitializeHealth(data);
        }

        private void FixedUpdate()
        {
            _enemyAI.EnemyMove(_speed);
        }

        private void LateUpdate()
        {
            _enemyAI.FlipSprite();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.collider.CompareTag("Player")) return;
            col.collider.GetComponent<PlayerStatus>().GetHit(_damage, GameManager.Instance.player.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Area")) return;
            _enemyAI.EnemyReLocation();
        }
    }
}