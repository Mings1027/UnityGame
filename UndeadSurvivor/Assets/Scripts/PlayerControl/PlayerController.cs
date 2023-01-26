using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        private readonly int _run = Animator.StringToHash("Run");

        public static Rigidbody2D Rigid { get; private set; }

        private SpriteRenderer _sprite;
        private Animator _anim;
        private Vector2 _moveVec;
        private bool _isMove, _isAttack;

        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            Rigid = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            if (_isMove) PlayerMove();
        }

        private void LateUpdate()
        {
            PlayerAnimation();
            Flip();
        }

        public void MoveInput(InputAction.CallbackContext context)
        {
            _moveVec = context.ReadValue<Vector2>();
            _isMove = _moveVec.sqrMagnitude > 0;
        }

        private void PlayerMove()
        {
            Rigid.MovePosition(Rigid.position + moveSpeed * Time.fixedDeltaTime * _moveVec);
        }

        private void PlayerAnimation()
        {
            _anim.SetBool(_run, _isMove);
        }

        private void Flip()
        {
            if (_moveVec.x == 0) return;
            _sprite.flipX = _moveVec.x < 0;
        }
    }
}