using GameControl;
using PlayerControl;
using UnityEngine;

namespace EnemyControl
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAI : MonoBehaviour
    {
        private Rigidbody2D _rigid;
        private SpriteRenderer _sprite;
        private Animator _anim;
        private Health _enemyHealth;
        private Rigidbody2D _target;

        private float _speed;
        private int _damage;
        [SerializeField] private RuntimeAnimatorController[] animController;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
            _enemyHealth = GetComponent<Health>();
            _target = PlayerController.Rigid.GetComponent<Rigidbody2D>();
        }

        public void EnemyMove()
        {
            var rigidPosition = _rigid.position;
            var dirVec = _target.position - rigidPosition;
            var moveVec = dirVec.normalized * (_speed * Time.fixedDeltaTime);
            _rigid.MovePosition(rigidPosition + moveVec);
        }

        public void FlipSprite()
        {
            _sprite.flipX = _target.position.x < _rigid.position.x;
        }

        public void EnemyReLocation()
        {
            var dir = (Vector3)_target.position - transform.position;
            transform.Translate(dir * 2);
        }

        public void EnemyDamaged(Collision2D col)
        {
            col.collider.GetComponent<Health>().GetHit(_damage, _target.gameObject);
        }

        public void Init(SpawnData data)
        {
            _anim.runtimeAnimatorController = animController[data.spriteType];
            _speed = data.speed;
            _damage = data.damage;
            _enemyHealth.InitializeHealth(data);
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}