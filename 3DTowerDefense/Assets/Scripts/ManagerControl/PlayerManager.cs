using System;
using System.Collections.Generic;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class PlayerManager : MonoBehaviour
    {
        private Camera _cam;

        //OnMove LateUpdate
        private bool _isMoving;
        private Vector2 _camMoveVec;

        //OnRotate
        private bool _isRotating;
        private float _camRotateValue;
        private Vector3 _rotateVec;
        private Tweener _rotateTween;

        //OnCursorPosition
        private Ray _camRay;
        private Vector3 _cursorPos;
        private Vector3 _mousePos;

        //MoveCursor TowerSpawn
        private RaycastHit _hit;

        [SerializeField] private BuildingManager buildingManager;

        [SerializeField] private LayerMask cursorMoveLayer;


        [SerializeField] private Transform cursorObj;
        [SerializeField] private int moveSpeed;
        [Range(0, 1)] [SerializeField] private float rotateDuration;
        [SerializeField] private Ease rotateEase;

        private void Awake()
        {
            _cam = Camera.main;
            _rotateVec = new Vector3(0, 45, 0);
            _rotateTween = transform.DORotate(_rotateVec, rotateDuration)
                .SetAutoKill(false)
                .SetEase(rotateEase)
                .OnComplete(() => _isRotating = false);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _camMoveVec = context.ReadValue<Vector2>();
            _isMoving = _camMoveVec.sqrMagnitude > 0;
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            if (_isRotating) return;
            _isRotating = context.started;
            _camRotateValue = context.ReadValue<float>();
            CameraRotation();
        }

        private void CameraRotation()
        {
            _rotateVec += new Vector3(0, _camRotateValue * 90, 0);
            _rotateTween.ChangeEndValue(_rotateVec, rotateDuration, true).Restart();
        }

        public void OnMoveCursorPosition(InputAction.CallbackContext context)
        {
            _camRay = _cam.ScreenPointToRay(context.ReadValue<Vector2>());

            MoveCursor();
            buildingManager.TowerFollowMouse(_cursorPos);
        }

        private void MoveCursor()
        {
            if (Physics.Raycast(_camRay, out _hit, Mathf.Infinity, cursorMoveLayer))
            {
                _cursorPos.x = Mathf.Round(_hit.point.x);
                _cursorPos.y = Mathf.Round(_hit.point.y);
                _cursorPos.z = Mathf.Round(_hit.point.z);

                cursorObj.position = _cursorPos;
            }
        }

        public void OnBuild(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                buildingManager.BuildTower(_hit);
            }
        }

        private void LateUpdate()
        {
            if (_isMoving)
            {
                var moveVec = new Vector3(_camMoveVec.x, 0, _camMoveVec.y);
                var angle = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg + transform.eulerAngles.y;
                moveVec = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                transform.Translate(moveVec * (Time.deltaTime * moveSpeed), Space.World);
            }
        }
    }
}