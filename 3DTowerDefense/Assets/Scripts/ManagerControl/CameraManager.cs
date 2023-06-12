using System;
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
        private Vector3 _touchStartPos;
        private bool _isLeft;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private Vector2Int scale;
        [SerializeField] private Vector2Int camPosLimit;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void OnEnable()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;

            switch (Input.touchCount)
            {
                case 1:
                    CameraControl();
                    break;
                case 2:
                    CameraZoom();
                    break;
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void CameraControl()
        {
            var touch = Input.GetTouch(0);
          
            if (touch.phase == TouchPhase.Began)
            {
                _lerp = 1;
                _isLeft = touch.position.x < Screen.width * 0.5f;
            }

            if (_isLeft)
            {
                CameraMove(touch);
            }
            else
            {
                CameraRotate(touch);
            }
        }

        private void CameraMove(Touch touch)
        {
            if (touch.phase == TouchPhase.Moved)
            {
                Moving(touch);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                MovingAsync(touch).Forget();
            }
        }

        private void Moving(Touch touch)
        {
            var t = transform;
            var pos = t.right * (touch.deltaPosition.x * -moveSpeed);
            pos += t.forward * (touch.deltaPosition.y * -moveSpeed);
            pos.y = 0;
            var newPos = t.position + pos * Time.deltaTime;

            newPos.x = Mathf.Clamp(newPos.x, -camPosLimit.x, camPosLimit.x);
            newPos.z = Mathf.Clamp(newPos.z, -camPosLimit.y, camPosLimit.y);

            transform.position = newPos;
        }

        private async UniTaskVoid MovingAsync(Touch touch)
        {
            _lerp = 0;
            while (_lerp < 1)
            {
                _lerp += Time.deltaTime * moveSpeed;
                Moving(touch);
                await UniTask.Yield(cancellationToken: _cts.Token);
            }

        }

        private void CameraRotate(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Moved:
                {
                    var t = transform;
                    t.Rotate(new Vector3(0.0f, touch.deltaPosition.x * rotationSpeed, 0.0f));
                    transform.rotation = Quaternion.Euler(0.0f, t.rotation.eulerAngles.y, 0.0f);
                    break;
                }
                case TouchPhase.Ended:
                    transform.DORotate(SnappedVector(), 0.5f)
                        .SetEase(Ease.OutBounce);
                    break;
                case TouchPhase.Began:
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector3 SnappedVector()
        {
            var currentY = Mathf.Ceil(transform.rotation.eulerAngles.y);
            var endValue = currentY switch
            {
                <= 45 or >= 315 => 0f,
                >= 45 and <= 135 => 90f,
                >= 136 and <= 225 => 180f,
                _ => 270f
                // >= 0 and <= 90 => 45.0f,
                // >= 91 and <= 180 => 135.0f,
                // >= 181 and <= 270 => 225.0f,
                // _ => 315.0f
            };

            return new Vector3(0.0f, endValue, 0.0f);
        }

        private void CameraZoom()
        {
            var firstTouch = Input.GetTouch(0);
            var secondTouch = Input.GetTouch(1);

            var firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
            var secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;

            var touchPrevPosDiff = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
            var touchCurPosDiff = (firstTouch.position - secondTouch.position).magnitude;

            var zoomModifier = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * zoomSpeed;

            if (touchPrevPosDiff > touchCurPosDiff)
            {
                _cam.orthographicSize += zoomModifier;
            }
            else
            {
                _cam.orthographicSize -= zoomModifier;
            }

            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, scale.x, scale.y);
        }
    }
}