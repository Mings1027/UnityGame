using UnityEngine;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private bool _isMoving;
        private bool _isRotate;
        private Vector2 _camMoveVec;
        private float _camRotateValue;

        [SerializeField] private InputManager input;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            input.onCameraMoveEvent += CameraMove;
            input.onCameraRotateEvent += CameraRotate;
        }

        private void CameraMove(Vector2 moveVec)
        {
            _isMoving = moveVec.sqrMagnitude > 0;
            _camMoveVec = moveVec;
        }

        private void CameraRotate(float rotValue)
        {
            _isRotate = rotValue != 0;
            _camRotateValue = rotValue;
        }

        private void LateUpdate()
        {
            if (_isMoving)
            {
                var vec = new Vector3(_camMoveVec.x, 0, _camMoveVec.y);
                var angle = Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
                vec = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                transform.Translate(vec * (Time.deltaTime * moveSpeed), Space.World);
            }

            if (_isRotate)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * _camRotateValue * rotationSpeed);
            }
        }
    }
}