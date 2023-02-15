using System;
using System.Linq;
using TowerDefenseInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputManager : ScriptableObject, GameInput.IGamePlayActions, GameInput.IUIActions
    {
        private GameInput _gameInput;
        private event Action<InputActionMap> OnActionMapChange;

        public bool isBuild, isEdit;

//===================================Game Play===========================================
        public event Action<Vector2> OnCameraMoveEvent;
        public event Action<float> OnCameraRotateEvent;
        public event Action<Vector2> OnCursorPositionEvent;

        public event Action OnPauseEvent;
        public event Action OnClickTowerEvent;

        public event Action OnCancelPanelEvent;

        // public event Action OnBuildTowerEvent, OnCancelBuildEvent;
        //
        // public event Action OnCancelEditEvent;

//=======================================UI===========================================
        public event Action OnResumeEvent;

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
            ToggleActionMap(_gameInput.GamePlay);
            isBuild = false;
            isEdit = false;
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
            OnCameraMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraRotate(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnCameraRotateEvent?.Invoke(context.ReadValue<float>());
            }
        }

        public void OnCursorPosition(InputAction.CallbackContext context)
        {
            OnCursorPositionEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (UiManager.Pointer) return;
            if (context.started)
            {
                OnCancelPanelEvent?.Invoke();
                // if (isBuild)
                // {
                //     OnCancelBuildEvent?.Invoke();
                // }
                // else if (isEdit)
                // {
                //     OnCancelEditEvent?.Invoke();
                // }
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnPauseEvent?.Invoke();
                ToggleActionMap(_gameInput.UI);
                // if (isBuild)
                // {
                //     OnCancelBuildEvent?.Invoke();
                // }
                // else if (isEdit)
                // {
                //     OnCancelEditEvent?.Invoke();
                // }
                // else
                // {
                //     OnPauseEvent?.Invoke();
                //     ToggleActionMap(_gameInput.UI);
                // }
            }
        }

        //==================================UI Action Map=============================================

        public void Resume()
        {
            ToggleActionMap(_gameInput.GamePlay);
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnResumeEvent?.Invoke();
                ToggleActionMap(_gameInput.GamePlay);
            }
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
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

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnRightClick(InputAction.CallbackContext context)
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