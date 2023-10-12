using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _cam;
        private CancellationTokenSource _cts;

        private float _lerp;
        private float _modifiedMoveSpeed;

        private bool isRotating;
        private Touch _firstTouch, _secondTouch;
        private Vector3 _curPos, _newPos;

        [SerializeField] private float rotationSpeed;
        [SerializeField] private float zoomSpeed;

        [SerializeField] private Vector2Int camSizeMinMax;
        [SerializeField] private Vector2Int camPosLimit;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            transform.rotation = Quaternion.Euler(0, 45, 0);
            _cam.orthographic = true;
        }

        private void LateUpdate()
        {
            if (Input.touchCount <= 0) return;

            if (Input.touchCount == 1)
            {
                if (isRotating) return;
                CameraRotate();
            }

            if (Input.touchCount == 2)
            {
                _firstTouch = Input.GetTouch(0);
                _secondTouch = Input.GetTouch(1);

                CameraMovement();
                CameraZoom();
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private bool IsInBounds(Vector3 position)
        {
            return position.x > -camPosLimit.x &&
                   position.x < camPosLimit.x &&
                   position.z > -camPosLimit.y &&
                   position.z < camPosLimit.y;
        }

        private void CameraMovement()
        {
            if (_firstTouch.phase == TouchPhase.Moved || _secondTouch.phase == TouchPhase.Moved)
            {
                CamMove();
            }
            else if (_firstTouch.phase == TouchPhase.Ended || _secondTouch.phase == TouchPhase.Ended)
            {
                MovingAsync().Forget();
            }
        }

        private void CamMove()
        {
            var pos = (_firstTouch.deltaPosition + _secondTouch.deltaPosition) * 0.5f;
            var t = transform;
            _curPos = t.right * (-_modifiedMoveSpeed * pos.x);
            _curPos += t.forward * (pos.y * -_modifiedMoveSpeed);
            _curPos.y = 0;
            _newPos = t.position + _curPos * Time.deltaTime;

            if (IsInBounds(_newPos))
            {
                t.position = _newPos;
            }
        }

        private async UniTaskVoid MovingAsync()
        {
            _lerp = 0;
            while (_lerp < 1)
            {
                _lerp += Time.deltaTime * _modifiedMoveSpeed;
                CamMove();
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
        }

        private void CameraRotate()
        {
            _firstTouch = Input.GetTouch(0);
            if (_firstTouch.phase == TouchPhase.Moved)
            {
                var t = transform;
                t.Rotate(new Vector3(0.0f, _firstTouch.deltaPosition.x * rotationSpeed, 0.0f));
                transform.rotation = Quaternion.Euler(0.0f, t.rotation.eulerAngles.y, 0.0f);
            }
            else if (_firstTouch.phase == TouchPhase.Ended)
            {
                isRotating = true;
                transform.DORotate(SnappedVector(), 0.5f)
                    .SetEase(Ease.OutBounce).SetLink(gameObject).OnComplete(() => isRotating = false);
            }
        }

        private Vector3 SnappedVector()
        {
            var currentY = Mathf.Ceil(transform.rotation.eulerAngles.y);
            var endValue = currentY switch
            {
                // <= 45 or >= 315 => 0f,
                // >= 45 and <= 135 => 90f,
                // >= 136 and <= 225 => 180f,
                // _ => 270f
                >= 0 and <= 90 => 45.0f,
                >= 91 and <= 180 => 135.0f,
                >= 181 and <= 270 => 225.0f,
                _ => 315.0f
            };

            return new Vector3(0.0f, endValue, 0.0f);
        }

        private void CameraZoom()
        {
            _firstTouch = Input.GetTouch(0);
            _secondTouch = Input.GetTouch(1);

            var firstTouchPrevPos = _firstTouch.position - _firstTouch.deltaPosition;
            var secondTouchPrevPos = _secondTouch.position - _secondTouch.deltaPosition;

            var touchPrevPosDiff = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            var touchCurPosDiff = (_firstTouch.position - _secondTouch.position).magnitude;

            var zoomModifier = (_firstTouch.deltaPosition - _secondTouch.deltaPosition).magnitude * zoomSpeed;

            var sizeChange = touchPrevPosDiff > touchCurPosDiff ? zoomModifier : -zoomModifier;

            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + sizeChange, camSizeMinMax.x, camSizeMinMax.y);

            _modifiedMoveSpeed = _cam.orthographicSize / 20;
        }
    }
}