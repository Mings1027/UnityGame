using System;
using TowerDefenseInput;
using UIControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputManager : ScriptableObject, GameInput.IGamePlayActions, GameInput.IUIActions
    {
        private Camera _cam;
        private GameInput _gameInput;

        private Ray _ray;
        // private event Action<InputActionMap> OnActionMapChange;

        public Vector2 mousePos;
        public bool isMoveUnit;

//===================================Game Play===========================================
        public event Action<Vector2> onCameraMoveEvent;
        public event Action<float> onCameraRotateEvent;
        public event Action onPauseEvent;
        public event Action onMoveUnitEvent;
        public event Action onClosePanelEvent;

//=======================================UI===========================================
        public event Action onResumeEvent;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.GamePlay.SetCallbacks(this);
                _gameInput.UI.SetCallbacks(this);
            }

            Init();
        }

        private void Init()
        {
            _cam = Camera.main;

            mousePos = Vector2.zero;
            isMoveUnit = false;

            ToggleActionMap(_gameInput.GamePlay);
        }

        private void ToggleActionMap(InputActionMap inputActionMap)
        {
            if (inputActionMap.enabled) return;
            _gameInput.Disable();
            // OnActionMapChange?.Invoke(inputActionMap);
            inputActionMap.Enable();
        }

        //==================================Game Player Action Map=============================================

        public void OnCameraMove(InputAction.CallbackContext context)
        {
            onCameraMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraRotate(InputAction.CallbackContext context)
        {
            onCameraRotateEvent?.Invoke(context.ReadValue<float>());
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            onPauseEvent?.Invoke();
            ToggleActionMap(_gameInput.UI);
        }

        public void OnMousePosition(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _ray = _cam.ScreenPointToRay(context.ReadValue<Vector2>());

                mousePos = context.ReadValue<Vector2>();
            }
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (!UIManager.pointer)
            {
                if (context.canceled)
                {
                    if (isMoveUnit)
                    {
                        onMoveUnitEvent?.Invoke();
                    }
                    else if (Physics.Raycast(_ray, out var hit))
                    {
                        if (hit.collider.CompareTag("Ground"))
                            onClosePanelEvent?.Invoke();
                    }
                }
            }
        }

        //==================================UI Action Map=============================================

        public void OnResume(InputAction.CallbackContext context)
        {
            onResumeEvent?.Invoke();
            ToggleActionMap(_gameInput.GamePlay);
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnUIClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }


        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnUIRightClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }


        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
    }
}