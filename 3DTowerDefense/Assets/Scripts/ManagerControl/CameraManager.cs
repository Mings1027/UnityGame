using UnityEngine;
using UnityEngine.InputSystem;

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
        public float rotLerp;

        [SerializeField] private Transform cameraArm;
        [SerializeField] private int rotationSpeed;
        [SerializeField] private int moveSpeed;

        private void Awake()
        {
            rotLerp = 1;
            cameraArm.rotation = Quaternion.Euler(0, 45, 0);
            _camRotQuaternion = cameraArm.rotation;
        }

        public void OnCameraMove(InputAction.CallbackContext context)
        {
            _camMoveVec = context.ReadValue<Vector2>();
            _isMoving = _camMoveVec.sqrMagnitude > 0;
        }

        public void OnCameraRotate(InputAction.CallbackContext context)
        {
            if (context.started && rotLerp >= 1)
            {
                rotLerp = 0;
                _camRotQuaternion *= Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up);
            }
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

            if (rotLerp < 1)
            {
                rotLerp += Time.deltaTime * rotationSpeed;
                cameraArm.rotation = Quaternion.Lerp(cameraArm.rotation, _camRotQuaternion, rotLerp);
            }
        }
    }
}