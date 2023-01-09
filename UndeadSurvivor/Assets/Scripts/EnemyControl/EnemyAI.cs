using GameControl;
using UnityEngine;

namespace EnemyControl
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAI : MonoBehaviour
    {
        private Rigidbody2D _rigid;
        private SpriteRenderer _sprite;
        private Rigidbody2D _target;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _target = GameManager.Instance.player.GetComponent<Rigidbody2D>();
        }
        
        public void EnemyMove(float speed)
        {
            var rigidPosition = _rigid.position;
            var dirVec = _target.position - rigidPosition;
            var moveVec = dirVec.normalized * (speed * Time.fixedDeltaTime);
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

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}