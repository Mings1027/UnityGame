using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        //OnMove LateUpdate
        private bool _isMoving;
        private Vector2 _camMoveVec;

        //OnRotate
        private Vector3 _rotateVec;
        private Quaternion _camRotQuaternion;
        private float _rotLerp;

        [SerializeField] private InputManager input;
        [SerializeField] private Transform cameraArm;
        [SerializeField] private int rotationSpeed;
        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            input.OnCameraMoveEvent += CameraMove;
            input.OnCameraRotateEvent += CameraRotate;
            _rotLerp = 1;
            cameraArm.rotation = Quaternion.Euler(0, 45, 0);
            _camRotQuaternion = cameraArm.rotation;
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
                var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + cameraArm.eulerAngles.y;
                moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                cameraArm.Translate(moveVec * (Time.deltaTime * moveSpeed), Space.World);
            }

            if (_rotLerp < 1)
            {
                _rotLerp += Time.deltaTime * rotationSpeed;
                cameraArm.rotation = Quaternion.Lerp(cameraArm.rotation, _camRotQuaternion, _rotLerp);
            }
        }
    }
}