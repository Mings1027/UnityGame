using DG.Tweening;
using UnityEngine;

namespace TestControl
{
    public class Test : MonoBehaviour
    {
        private Camera cam;
        private Quaternion rotation;
        private Vector3 curPos;
        private Vector3 originPos;
        private Tweener moveTween;
        [SerializeField] private Vector3 smoothSway;
        [SerializeField] private Vector2 limitPos;
        [SerializeField] private float smoothSpeed;
        [SerializeField] private float lookSensitivity;
        [SerializeField] private float currentCameraRotationX;
        [SerializeField] private float cameraRotationLimit;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Start()
        {
            moveTween = transform.DOLocalMove(originPos, 1).SetAutoKill(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            CamRotation();

            if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
            {
                // Swaying();
                moveTween.ChangeEndValue(Vector3.zero, 1, true).Restart();
            }
            else
            {
                moveTween.ChangeEndValue(Vector3.zero, 1, true).Restart();
                // BackTo();
            }
        }

        private void Swaying()
        {
            var moveX = Input.GetAxis("Mouse X") * 20;
            var moveY = Input.GetAxis("Mouse Y") * 20;

            curPos.Set(Mathf.Clamp(Mathf.Lerp(curPos.x, -moveX, smoothSway.x), -limitPos.x, limitPos.x),
                Mathf.Clamp(Mathf.Lerp(curPos.y, -moveY, smoothSway.y), -limitPos.y,
                    limitPos.y), originPos.z);

            transform.localPosition = curPos;
        }

        private void BackTo()
        {
            curPos = Vector3.Lerp(curPos, originPos, smoothSway.x);
            transform.localPosition = curPos;
        }

        private void CamRotation()
        {
            var xRot = Input.GetAxisRaw("Mouse Y");
            float _cameraRotationX = xRot * lookSensitivity;

            currentCameraRotationX -= _cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }
}