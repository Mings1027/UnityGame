using System;
using System.Collections.Generic;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControl
{
    public class PlayerManager : MonoBehaviour
    {
        private Vector2 _camMove, _camRotate, _mousePos;
        private Vector3 _rotVec;

        private bool _isMoving, _isRotating;

        private Tweener _rotateTween;
        private Camera _cam;
        private Vector3 _cursorPos;

        private Ray _camRay;
        private RaycastHit _hit;

        private readonly List<GameObject> _towerList = new();

        [SerializeField] private Transform cursorObj;

        [SerializeField] private int moveSpeed;
        [Range(0, 1)] [SerializeField] private float rotateDuration;
        [SerializeField] private Ease rotateEase;

        private void Awake()
        {
            _cam = Camera.main;
            _rotVec = new Vector3(0, 45, 0);
            _rotateTween = transform.DORotate(_rotVec, rotateDuration)
                .SetAutoKill(false)
                .SetEase(rotateEase)
                .OnComplete(() => _isRotating = false);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _camMove = context.ReadValue<Vector2>();
            _isMoving = _camMove.sqrMagnitude > 0;
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (_isRotating) return;
            _isRotating = context.started;
            _camRotate = context.ReadValue<Vector2>();
            CameraRotation();
        }

        public void OnCursorPosition(InputAction.CallbackContext context)
        {
            _mousePos = context.ReadValue<Vector2>();
            _camRay = _cam.ScreenPointToRay(_mousePos);
            MoveCursor();
        }

        public void OnSpawn(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                TowerSpawn();
            }
        }
        // Camera Zoom
        // public void OnWheel(InputAction.CallbackContext context)
        // {
        //     var a = context.ReadValue<Vector2>();
        //     if (a.y > 0)
        //     {
        //         _cam.orthographicSize += 1;
        //     }
        //     else if (a.y < 0)
        //     {
        //         _cam.orthographicSize -= 1;
        //     }
        // }

        private void CameraRotation()
        {
            _rotVec += new Vector3(0, _camRotate.x * 90, 0);
            _rotateTween.ChangeEndValue(_rotVec, rotateDuration, true).Restart();
        }

        private void MoveCursor()
        {
            if (!Physics.Raycast(_camRay, out _hit, Mathf.Infinity)) return;
            _cursorPos.x = Mathf.Round(_hit.point.x);
            _cursorPos.y = Mathf.Round(_hit.point.y);
            _cursorPos.z = Mathf.Round(_hit.point.z);

            cursorObj.position = _cursorPos;
        }

        private void TowerSpawn()
        {
            if (_hit.collider.CompareTag("Ground"))
            {
                _towerList.Add(StackObjectPool.Get("Tower", cursorObj.position));
            }
            else
                print("Wrong Place");
        }

        public void TowerReset()
        {
            foreach (var t in _towerList)
            {
                t.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (!_isMoving) return;
            var moveVec = new Vector3(_camMove.x, 0, _camMove.y);
            var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
            moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            transform.Translate(moveVec * (Time.deltaTime * moveSpeed), Space.World);
        }
    }
}