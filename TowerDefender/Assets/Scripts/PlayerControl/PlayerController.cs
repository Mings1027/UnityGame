using System;
using UnityEngine;

namespace PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        private Camera _cam;
        private Rigidbody _rigid;
        private CharacterController _characterController;
        private PlayerInputController _input;
        private float _x, _y;
        public Vector3 moveVec;
        public Vector3 changedMoveVec;

        [Range(0, 10)] [SerializeField] private float moveSpeed;

        [Header("CameraControl")] [SerializeField]
        private Transform cameraTarget;

        [Range(0, 10)] [SerializeField] private float xSpeed, ySpeed;
        [SerializeField] private float topLimit = 50, bottomLimit = 20;

        private bool _isMove;

        private void Awake()
        {
            _cam = Camera.main;
            _rigid = GetComponent<Rigidbody>();
            _characterController = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputController>();
        }

        private void Update()
        {
            PlayerMoveInput();
        }

        private void FixedUpdate()
        {
            // PlayerMove();
            PlayerMoooove();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }


        private void PlayerMoveInput()
        {
            var inputVec = _input.Move;
            moveVec = new Vector3(inputVec.x, 0, inputVec.y);
        }

        private void PlayerMove()
        {
            _isMove = moveVec.sqrMagnitude > 0f;
            if (!_isMove) return;
            var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + _cam.transform.eulerAngles.y;
            moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            _characterController.Move(moveSpeed * Time.fixedDeltaTime * moveVec);
        }

        private void PlayerMoooove()
        {
            changedMoveVec = Quaternion.AngleAxis(_cam.transform.rotation.eulerAngles.y, Vector3.up) * moveVec;
            // _characterController.Move(speed * Time.fixedDeltaTime * changedMoveVec);
            // _characterController.SimpleMove(moveSpeed * changedMoveVec);
            _rigid.MovePosition(_rigid.position + changedMoveVec * (moveSpeed * Time.fixedDeltaTime));
        }

        private void CameraRotation()
        {
            if (_input.Look.sqrMagnitude > 0.01f)
            {
                _x += _input.Look.x * xSpeed;
                _y += _input.Look.y * ySpeed;
            }

            _x = Mathf.Clamp(_x, float.MinValue, float.MaxValue); //ClampAngle(_x, float.MinValue, float.MaxValue);
            _y = Mathf.Clamp(_y, bottomLimit, topLimit); //ClampAngle(_y, bottomLimit, topLimit);

            cameraTarget.transform.rotation = Quaternion.Euler(_y, _x, 0);
        }
    }
}