using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private bool _isMoving;
        private Vector2 _camMoveVec;

        private Vector3 _rotateVec;
        private Quaternion _camRotQuaternion;
        private float _rotLerp;

        [SerializeField] private InputManager input;
        [SerializeField] private int rotationSpeed;
        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            input.onCameraMoveEvent += CameraMove;
            input.onCameraRotateEvent += CameraRotate;
            _rotLerp = 1;
            _camRotQuaternion = Quaternion.Euler(0, 0, 0);
            transform.rotation = _camRotQuaternion;
        }

        private void CameraMove(Vector2 moveVec)
        {
            _camMoveVec = moveVec;
            _isMoving = _camMoveVec.sqrMagnitude > 0;
        }

        private void CameraRotate(float rotValue)
        {
            _rotLerp = 0;
            _camRotQuaternion *= Quaternion.AngleAxis(90 * rotValue, Vector3.up);
        }

        private void LateUpdate()
        {
            if (_isMoving)
            {
                var moveVec = new Vector3(_camMoveVec.x, 0, _camMoveVec.y);
                var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
                moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                transform.Translate(moveVec * (Time.deltaTime * moveSpeed), Space.World);
            }

            if (_rotLerp < 1)
            {
                _rotLerp += Time.deltaTime * rotationSpeed;
                transform.rotation = Quaternion.Lerp(transform.rotation, _camRotQuaternion, _rotLerp);
            }
        }
    }
}