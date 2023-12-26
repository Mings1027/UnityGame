using System;
using System.Diagnostics;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ManagerControl
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _cam;
        private Tweener _snapRotateTween;
        private CamState _camState;

        private Touch _firstTouch;

        private bool _startSmoothStop;
        private float _decreaseSpeed;
        private float _modifiedMoveSpeed;
        private Vector3 _curPos, _newPos;
        private Vector2 _t1T2Vec;
        private Vector2 _firstTouchPos, _secondTouchPos;

        public event Action OnResizeUIEvent;

        [SerializeField, Range(1, 3)] private float moveSpeed;
        [SerializeField, Range(1, 3)] private float zoomSpeed;

        [SerializeField, Range(0, 2)] private float decreaseMoveSpeed;

        [SerializeField, Range(5, 20)] private int movingStartOrthoSize;
        [SerializeField, Range(0, 90)] private byte dragAngle;
        [SerializeField] private float modifiedDivide;

        [SerializeField] private byte movementBound;

        [SerializeField] private Vector3 minCamPos, maxCamPos;
        [SerializeField] private Vector2Int orthoSizeMinMax;

        [SerializeField] private Vector2 uiSizeMinMax;

// #if UNITY_EDITOR
//         [Header("On Validate"), SerializeField]
//         private float cameraPosition;
// #endif
        private void Awake()
        {
            _cam = Camera.main;
            _snapRotateTween = transform.DORotate(SnappedVector(), 0.5f).SetEase(Ease.OutCubic)
                .SetAutoKill(false).Pause();
        }

        // [Conditional("UNITY_EDITOR")]
        // private void OnValidate()
        // {
        //     var cam = transform.GetChild(0);
        //     var camPos = cam.transform.localPosition;
        //     camPos.x = 0;
        //     camPos.y = cameraPosition;
        //     camPos.z = -cameraPosition;
        //     cam.transform.localPosition = camPos;
        // }

        private void OnEnable()
        {
            Input.multiTouchEnabled = true;
            _modifiedMoveSpeed = _cam.orthographicSize / modifiedDivide;
        }

        private void Start()
        {
            _camState = CamState.Idle;
            transform.rotation = Quaternion.Euler(0, 45, 0);
            enabled = false;
        }

        private void Update()
        {
            switch (_camState)
            {
                case CamState.Idle:
                    Idle();
                    break;
                case CamState.Move:
                    CamMovement();
                    break;
                case CamState.CheckZoomRotate:
                    CheckZoomRotate();
                    break;
                case CamState.Zoom:
                    CamZoom();
                    break;
                case CamState.Rotate:
                    CamRotate();
                    break;
            }
        }

        private void OnDisable()
        {
            Input.multiTouchEnabled = false;
        }

        private void Idle()
        {
            if (Input.touchCount != 0)
            {
                if (Input.touchCount == 1)
                {
                    var touch1 = Input.GetTouch(0);
                    if (touch1.phase == TouchPhase.Began)
                    {
                        _startSmoothStop = false;
                        _firstTouchPos = touch1.position;
                        if (touch1.position.y < Screen.height * 0.1f) return;
                        _camState = CamState.Move;
                    }
                }
                else if (Input.touchCount == 2)
                {
                    var touch1 = Input.touches[0];
                    var touch2 = Input.touches[1];
                    if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
                    {
                        _firstTouchPos = touch1.position;
                        _secondTouchPos = touch2.position;
                        _camState = CamState.CheckZoomRotate;
                    }
                }
            }
            else if (_startSmoothStop)
            {
                if (_decreaseSpeed > 0)
                {
                    _decreaseSpeed -= Time.deltaTime * decreaseMoveSpeed;
                    CamMove(_decreaseSpeed);
                }
                else
                {
                    _startSmoothStop = false;
                }
            }
        }

        private void CamMovement()
        {
            if (Input.touchCount == 1)
            {
                _firstTouch = Input.touches[0];

                if (_firstTouch.phase == TouchPhase.Moved)
                {
                    CamMove(moveSpeed);
                }

                else if (_firstTouch.phase == TouchPhase.Ended)
                {
                    _camState = CamState.Idle;
                    if (_firstTouch.deltaPosition != Vector2.zero)
                    {
                        _startSmoothStop = true;
                        _decreaseSpeed = _modifiedMoveSpeed;
                    }
                }
            }

            else if (Input.touchCount == 2)
            {
                var touch = Input.touches[1];
                if (touch.phase == TouchPhase.Began)
                {
                    _secondTouchPos = touch.position;
                    _t1T2Vec = (_secondTouchPos - _firstTouchPos).normalized;
                    _camState = CamState.CheckZoomRotate;
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

        private bool IsInBounds(Vector3 position)
        {
            return position.x > -movementBound &&
                   position.x < movementBound &&
                   position.z > -movementBound &&
                   position.z < movementBound;
        }

        private void CheckZoomRotate()
        {
            if (Input.touchCount == 1)
            {
                var touch1 = Input.GetTouch(0);
                if (touch1.phase == TouchPhase.Began)
                {
                    _firstTouchPos = touch1.position;
                    _camState = CamState.Move;
                }
            }

            else if (Input.touchCount == 2)
            {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    var t1Angle = Vector2.Angle(_t1T2Vec, touch1.position - _firstTouchPos);
                    var t2Angle = Vector2.Angle(-_t1T2Vec, touch2.position - _secondTouchPos);
                    if (Vector2.Distance(_firstTouchPos, touch1.position) > 50 ||
                        Vector2.Distance(_secondTouchPos, touch2.position) > 50)
                    {
                        if (t1Angle > dragAngle && t1Angle < 180 - dragAngle ||
                            t2Angle > dragAngle && t2Angle < 180 - dragAngle)
                        {
                            _camState = CamState.Rotate;
                            _snapRotateTween.Pause();
                        }
                        else
                        {
                            _camState = CamState.Zoom;
                        }
                    }
                }
                else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                {
                    _camState = CamState.Move;
                }
            }
        }

        private void CamZoom()
        {
            if (Input.touchCount == 1)
            {
                var touch1 = Input.GetTouch(0);
                if (touch1.phase == TouchPhase.Began)
                {
                    _firstTouchPos = touch1.position;
                    _camState = CamState.Move;
                }
            }

            else if (Input.touchCount == 2)
            {
                var touch1 = Input.touches[0];
                var touch2 = Input.touches[1];
                if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    var touch0Cur = touch1.position;
                    var touch1Cur = touch2.position;

                    var touch0Prev = touch1.position - touch1.deltaPosition;
                    var touch1Prev = touch2.position - touch2.deltaPosition;

                    var prevDistance = Vector3.Magnitude(touch0Prev - touch1Prev);
                    var curDistance = Vector3.Magnitude(touch0Cur - touch1Cur);
                    var difference = curDistance - prevDistance;

                    var orthographicSize = Mathf.Clamp(_cam.orthographicSize - difference * Time.deltaTime * zoomSpeed,
                        orthoSizeMinMax.x, orthoSizeMinMax.y);
                    _cam.orthographicSize = orthographicSize;

                    var t = Mathf.InverseLerp(movingStartOrthoSize, orthoSizeMinMax.y, orthographicSize);
                    var newCamPos = Vector3.Lerp(minCamPos, maxCamPos, t);
                    _cam.transform.localPosition = newCamPos;
                    OnResizeUIEvent?.Invoke();
                }

                if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                {
                    _camState = CamState.Move;
                    _modifiedMoveSpeed = _cam.orthographicSize / modifiedDivide;
                }
            }
        }

        private void CamRotate()
        {
            if (Input.touchCount == 1)
            {
                var touch1 = Input.GetTouch(0);
                if (touch1.phase == TouchPhase.Began)
                {
                    _firstTouchPos = touch1.position;
                    _camState = CamState.Move;
                }
            }

            else if (Input.touchCount == 2)
            {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);

                if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    var prevPos1 = touch1.position - touch1.deltaPosition;
                    var prevPos2 = touch2.position - touch2.deltaPosition;

                    var prevDir = prevPos2 - prevPos1;
                    var curDir = touch2.position - touch1.position;

                    var angle = Vector2.SignedAngle(prevDir, curDir);
                    transform.Rotate(0, angle, 0);
                }

                if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                {
                    _camState = CamState.Move;
                    _snapRotateTween.ChangeStartValue(transform.rotation.eulerAngles)
                        .ChangeEndValue(SnappedVector())
                        .Restart();
                }
            }
        }

        private Vector3 SnappedVector()
        {
            var currentY = Math.Ceiling(transform.eulerAngles.y);
            var endValue = currentY switch
            {
                >= 0 and <= 90 => 45.0f,
                >= 91 and <= 180 => 135.0f,
                >= 181 and <= 270 => 225.0f,
                _ => 315.0f
            };
            return new Vector3(0.0f, endValue, 0.0f);
        }

        public void GameStartCamZoom()
        {
            transform.rotation = Quaternion.Euler(0, 45, 0);
            var t = Mathf.InverseLerp(movingStartOrthoSize, orthoSizeMinMax.y, _cam.orthographicSize);
            var newCamPos = Vector3.Lerp(minCamPos, maxCamPos, t);
            _cam.transform.localPosition = newCamPos;
            _modifiedMoveSpeed = _cam.orthographicSize / modifiedDivide;
        }

        public void ResizeUI(Transform statusBar)
        {
            var t = Mathf.InverseLerp(orthoSizeMinMax.y, orthoSizeMinMax.x, _cam.orthographicSize);
            var newScale = Vector3.Lerp(Vector3.one * uiSizeMinMax.x, Vector3.one * uiSizeMinMax.y, t);

            statusBar.localScale = newScale;
        }

        public void DisableForPlaceTower()
        {
            enabled = false;
            _startSmoothStop = false;
        }
    }
}