using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UIControl;
using UnityEngine;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _cam;
        private CancellationTokenSource _cts;

        private float _lerp;
        private float _modifiedMoveSpeed;

        private bool _isMoving;
        private bool _isRotating;
        private Touch _firstTouch, _secondTouch;
        private Vector3 _curPos, _newPos;

        public event Action OnResizeUIEvent;

        [SerializeField] private float rotationSpeed;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private float decreaseMoveSpeed;

        [SerializeField] private float orthoSize;
        [SerializeField] private Vector2Int camSizeMinMax;
        [SerializeField] private Vector2 uiSizeMinMax;
        [SerializeField] private Vector2Int camPosLimit;
        [SerializeField] private Vector3 minCamPos;
        [SerializeField] private Vector3 maxCamPos;


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

        private void OnEnable()
        {
            _modifiedMoveSpeed = _cam.orthographicSize / 20;
        }

        private void LateUpdate()
        {
            if (Input.touchCount <= 0) return;
            CameraMovement();

            if (Input.touchCount == 2)
            {
                _firstTouch = Input.GetTouch(0);
                _secondTouch = Input.GetTouch(1);
                CameraZoom();
                if (_isRotating) return;
                CameraRotate();
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
            if (Input.touchCount == 1)
            {
                _firstTouch = Input.GetTouch(0);

                if (_firstTouch.phase == TouchPhase.Moved)
                {
                    _isMoving = true;
                    CamMove(_modifiedMoveSpeed);
                }

                else if (_isMoving && _firstTouch.phase == TouchPhase.Ended)
                {
                    MovingAsync().Forget();
                }
            }
        }

        private void CamMove(float speed)
        {
            var pos = _firstTouch.deltaPosition * 0.5f;
            var t = transform;
            _curPos = t.right * (-speed * pos.x);
            _curPos += t.forward * (pos.y * -speed);
            _curPos.y = 0;
            _newPos = t.position + _curPos * Time.deltaTime;

            if (IsInBounds(_newPos))
            {
                t.position = _newPos;
            }
        }

        private async UniTaskVoid MovingAsync()
        {
            if (_firstTouch.deltaPosition.Equals(Vector2.zero)) return;
            var decreaseSpeed = _modifiedMoveSpeed;
            while (decreaseSpeed > 0)
            {
                decreaseSpeed -= Time.deltaTime * decreaseMoveSpeed;
                CamMove(decreaseSpeed);
                await UniTask.Yield();
            }

            _isMoving = false;
        }

        private void CameraRotate()
        {
            if (_firstTouch.phase == TouchPhase.Moved && _secondTouch.phase == TouchPhase.Moved)
            {
                var t = transform;
                var rotSpeed = _firstTouch.position.y > Screen.height * 0.5f ? -rotationSpeed : rotationSpeed;
                t.Rotate(new Vector3(0.0f, _firstTouch.deltaPosition.x * rotSpeed, 0.0f));
                transform.rotation = Quaternion.Euler(0.0f, t.rotation.eulerAngles.y, 0.0f);
            }
            else if (_firstTouch.phase == TouchPhase.Ended || _secondTouch.phase == TouchPhase.Ended)
            {
                _isRotating = true;
                transform.DORotate(SnappedVector(), 0.5f)
                    .SetEase(Ease.OutBounce).SetLink(gameObject).OnComplete(() => _isRotating = false);
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
            var firstTouchPrevPos = _firstTouch.position - _firstTouch.deltaPosition;
            var secondTouchPrevPos = _secondTouch.position - _secondTouch.deltaPosition;

            var touchPrevPosDiff = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            var touchCurPosDiff = (_firstTouch.position - _secondTouch.position).magnitude;

            var zoomModifier = (_firstTouch.deltaPosition - _secondTouch.deltaPosition).magnitude * zoomSpeed;

            var sizeChange = touchPrevPosDiff > touchCurPosDiff ? zoomModifier : -zoomModifier;

            var orthographicSize = Mathf.Clamp(_cam.orthographicSize + sizeChange, camSizeMinMax.x, camSizeMinMax.y);
            _cam.orthographicSize = orthographicSize;

            _modifiedMoveSpeed = orthographicSize / 20;

            var t = Mathf.InverseLerp(orthoSize, camSizeMinMax.y, orthographicSize);
            var newCamPos = Vector3.Lerp(minCamPos, maxCamPos, t);
            _cam.transform.localPosition = newCamPos;

            OnResizeUIEvent?.Invoke();
        }

        public async UniTask GameStartCamZoom()
        {
            await DOTween.Sequence().Append(_cam.transform.DOLocalMove(maxCamPos, 1.5f))
                .Join(_cam.DOOrthoSize(17, 1.5f).From(100).SetEase(Ease.OutQuad))
                .Join(_cam.transform.DOLocalMove(minCamPos, 1.5f));
            var t = Mathf.InverseLerp(orthoSize, camSizeMinMax.y, _cam.orthographicSize);
            var newCamPos = Vector3.Lerp(minCamPos, maxCamPos, t);
            _cam.transform.localPosition = newCamPos;
        }

        public void ResizeUI(Transform healthBarTransform)
        {
            var t = Mathf.InverseLerp(camSizeMinMax.y, camSizeMinMax.x, _cam.orthographicSize);
            var newScale = Vector3.Lerp(Vector3.one * uiSizeMinMax.x, Vector3.one * uiSizeMinMax.y, t);

            healthBarTransform.localScale = newScale;
        }
    }
}