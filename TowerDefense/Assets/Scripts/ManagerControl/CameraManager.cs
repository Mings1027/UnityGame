using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _cam;
        private CancellationTokenSource _cts;

        private float _lerp;

        private Touch _firstTouch, _secondTouch;
        private Vector3 prevPos, curPos, newPos;
        private Vector3 movePos;

        [SerializeField] private float moveSpeed;
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
        }

        private void LateUpdate()
        {
            if (Input.touchCount <= 0) return;

            if (Input.touchCount == 1)
            {
                CameraRotate();
            }

            CameraMovement();
            if (Input.touchCount == 2)
            {
                CameraZoom();
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        private void CameraMove()
        {
            _firstTouch = Input.GetTouch(0);
            _secondTouch = Input.GetTouch(1);

            if (_secondTouch.phase == TouchPhase.Moved)
            {
                var t = transform;
                var pos = t.right * (_secondTouch.deltaPosition.x * -moveSpeed);
                pos += t.forward * (_secondTouch.deltaPosition.y * -moveSpeed);
                pos.y = 0;
                newPos = t.position + pos * Time.deltaTime;

                newPos.x = Mathf.Clamp(newPos.x, -camPosLimit.x, camPosLimit.x);
                newPos.z = Mathf.Clamp(newPos.z, -camPosLimit.y, camPosLimit.y);

                transform.position = newPos;
            }
        }

        private void CameraMovement()
        {
            if (Input.touchCount == 2)
            {
                _firstTouch = Input.GetTouch(0);
                _secondTouch = Input.GetTouch(1);

                if (_firstTouch.phase == TouchPhase.Moved || _secondTouch.phase == TouchPhase.Moved)
                {
                    CamMoveTest();
                }
                else if (_firstTouch.phase == TouchPhase.Ended || _secondTouch.phase == TouchPhase.Ended)
                {
                    MovingAsync().Forget();
                }
            }
        }

        private void CamMoveTest()
        {
            var pos = (_firstTouch.deltaPosition + _secondTouch.deltaPosition) * 0.5f;
            var t = transform;
            curPos = t.right * (-moveSpeed * pos.x);
            curPos += t.forward * (pos.y * -moveSpeed);
            curPos.y = 0;
            newPos = t.position + curPos * Time.deltaTime;

            newPos.x = Mathf.Clamp(newPos.x, -camPosLimit.x, camPosLimit.x);
            newPos.z = Mathf.Clamp(newPos.z, -camPosLimit.y, camPosLimit.y);

            transform.position = newPos;
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

        private async UniTaskVoid MovingAsync()
        {
            _lerp = 0;
            while (_lerp < 1)
            {
                _lerp += Time.deltaTime * moveSpeed;
                CamMoveTest();
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
                transform.DORotate(SnappedVector(), 0.5f)
                    .SetEase(Ease.OutBounce);
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
        }
    }
}