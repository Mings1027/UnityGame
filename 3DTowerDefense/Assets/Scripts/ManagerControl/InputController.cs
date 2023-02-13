using System;
using System.Linq;
using TowerDefenseInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    [CreateAssetMenu(menuName = "InputReader")]
    public class InputController : ScriptableObject, GameInput.IGamePlayActions, GameInput.IUIActions,
        GameInput.IBuildModeActions
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

//=======================================UI===========================================
        public event Action OnResumeEvent;
        public event Action OnClickUIEvent;

//===================================Build Mode===========================================
        public event Action OnBuildTowerEvent, OnBuildCancelEvent;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.GamePlay.SetCallbacks(this);
                _gameInput.UI.SetCallbacks(this);
                _gameInput.BuildMode.SetCallbacks(this);
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

        public void OnBuildMode()
        {
            ToggleActionMap(_gameInput.BuildMode);
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

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Pause");
                ToggleActionMap(_gameInput.UI);
                OnPauseEvent?.Invoke();
            }
        }

        public void OnClickTower(InputAction.CallbackContext context)
        {
            if (context.started && !UiManager.OnPointer)
            {
                OnClickTowerEvent?.Invoke();
            }
        }


//==================================BuildMode Action Map=============================================

        public void OnBuildTower(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnBuildTowerEvent?.Invoke();
            }
        }

        public void OnBuildCancel(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnBuildCancelEvent?.Invoke();
            }
        }
//==================================UI Action Map=============================================

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                ToggleActionMap(_gameInput.GamePlay);
                OnResumeEvent?.Invoke();
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

        public void OnClick(InputAction.CallbackContext context)
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