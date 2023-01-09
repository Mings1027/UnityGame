using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int Run = Animator.StringToHash("Run");

        private Rigidbody2D _rigid;
        private SpriteRenderer _sprite;
        private Animator _anim;
        private Vector2 _moveVec;
        private bool _isMove, _isAttack;

        [SerializeField] private PlayerMeleeAttack meleeAttack;
        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody2D>();
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
            _rigid.MovePosition(_rigid.position + moveSpeed * Time.fixedDeltaTime * _moveVec);
        }

        private void PlayerAnimation()
        {
            _anim.SetBool(Run, _isMove);
        }

        private void Flip()
        {
            if (_moveVec.x == 0) return;
            _sprite.flipX = _moveVec.x < 0;
            if (!meleeAttack.enabled) return;
            var meleeRot = meleeAttack.transform.rotation;
            meleeRot.z = _sprite.flipX ? 180 : 0;
            meleeAttack.transform.rotation = meleeRot;
        }
    }
}