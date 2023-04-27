using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _cam;

        private float _xRotation;
        private bool _isBusy;

        private int _fingerId1 = -1, _fingerId2 = -1;
        private Vector2 _startPos1;
        private Vector2 _startPos2;
        private bool _isDrawing;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private float minScale, maxScale;

        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>();
        }

        private void LateUpdate()
        {
            CameraMove();
            SnappedRotate();
            // CameraZoom();
        }

        private void CameraMove()
        {
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    var pos = transform.right * (touch.deltaPosition.x * -moveSpeed);
                    pos += transform.up * (touch.deltaPosition.y * -moveSpeed);
                    transform.position += pos * Time.deltaTime;
                }
            }
        }

        private void SnappedRotate()
        {
            if (Input.touchCount == 2)
            {
                // 두 개의 손가락이 터치되면
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
                {
                    // 두 손가락이 처음 터치될 때
                    if (_fingerId1 == -1 && _fingerId2 == -1)
                    {
                        _fingerId1 = touch1.fingerId;
                        _fingerId2 = touch2.fingerId;
                        _startPos1 = touch1.position;
                        _startPos2 = touch2.position;
                        _isDrawing = true;
                    }
                }

                if (touch1.fingerId == _fingerId1 && touch2.fingerId == _fingerId2)
                {
                    // 두 손가락이 움직이는 중일 때
                    Vector2 currentPos1 = touch1.position;
                    Vector2 currentPos2 = touch2.position;

                    Vector2 dir1 = currentPos1 - _startPos1;
                    Vector2 dir2 = currentPos2 - _startPos2;

                    float angle = Vector2.Angle(dir1, dir2);
                    text.text = angle.ToString(CultureInfo.CurrentCulture);
                    if (angle is > 315f or < 45f)
                    {
                        print("rotate");
                        // 두 손가락의 이동 벡터가 45도 각도에 가까우면 원을 그리는 동작으로 간주합니다.
                        // 여기서는 angle이 315도보다 크거나 45도보다 작으면 원을 그리는 동작으로 처리합니다.
                        // 각도를 변경하여 원 그리는 동작의 정확도를 조절할 수 있습니다.
                        CameraRotation();
                        // 원을 그리는 동작을 처리하는 함수를 호출합니다.
                    }
                }
            }

            if (Input.touchCount < 2 && _isDrawing)
            {
                // 두 손가락 중 하나가 떼어지면 동작을 종료합니다.
                _fingerId1 = -1;
                _fingerId2 = -1;
                _isDrawing = false;
            }
        }

        private void CameraRotation()
        {
            transform.DORotate(SnappedVector(), 0.5f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() => _isBusy = false);
        }

        private Vector3 SnappedVector()
        {
            var currentY = Mathf.Ceil(transform.rotation.eulerAngles.y);
            var endValue = currentY switch
            {
                >= 0 and <= 90 => 45.0f,
                >= 91 and <= 180 => 135.0f,
                >= 181 and <= 270 => 225.0f,
                _ => 315.0f
            };

            return new Vector3(_xRotation, endValue, 0.0f);
        }

        private void CameraZoom()
        {
            if (Input.touchCount == 2)
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

                if (touchPrevPosDiff < touchCurPosDiff)
                {
                    _cam.orthographicSize -= zoomModifier;
                }

                _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, minScale, maxScale);
            }
        }
    }
}