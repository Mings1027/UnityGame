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

        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float zoomSpeed;
        [SerializeField] private float minScale, maxScale;

        [SerializeField] private float limitX;
        [SerializeField] private float limitZ;

        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>();
        }

        private void LateUpdate()
        {
            CameraMove();
            CameraRotate();
            CameraZoom();
        }

        private void CameraMove()
        {
            if (Input.touchCount == 2)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    var t = transform;
                    var pos = t.right * (touch.deltaPosition.x * -moveSpeed);
                    pos += t.forward * (touch.deltaPosition.y * -moveSpeed);
                    pos.y = 0;
                    var newPos = t.position + pos * Time.deltaTime;

                    newPos.x = Mathf.Clamp(newPos.x, -limitX, limitX);
                    newPos.z = Mathf.Clamp(newPos.z, -limitZ, limitZ);

                    transform.position = newPos;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    _isBusy = true;
                }
            }
        }

        private void CameraRotate()
        {
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    transform.Rotate(new Vector3(_xRotation, -touch.deltaPosition.x * rotationSpeed, 0.0f));
                    transform.rotation = Quaternion.Euler(_xRotation, transform.rotation.eulerAngles.y, 0.0f);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    transform.DORotate(SnappedVector(), 0.5f)
                        .SetEase(Ease.OutBounce)
                        .OnComplete(() => _isBusy = false);
                }
            }
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