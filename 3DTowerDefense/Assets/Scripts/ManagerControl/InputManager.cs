using System;
using TowerDefenseInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    [CreateAssetMenu(menuName = "InputManager")]
    public class InputManager : ScriptableObject, GameInput.IGamePlayActions, GameInput.IUIActions,
        GameInput.IBuildModeActions, GameInput.IEditModeActions
    {
        private GameInput _gameInput;

        private bool isBuild, isEdit;

        public event Action<Vector2> OnCameraMoveEvent;
        public event Action<float> OnCameraRotateEvent;

        public event Action<Vector2> OnCursorPositionEvent;

        public event Action OnPauseEvent, OnResumeEvent;

        public event Action OnBuildTowerEvent, OnSelectTowerEvent;
        public event Action OnBuildCancelEvent, OnSelectCancelEvent;

        private void OnEnable()
        {
            if (_gameInput != null) return;

            _gameInput = new GameInput();
            _gameInput.GamePlay.SetCallbacks(this);
            _gameInput.UI.SetCallbacks(this);
            _gameInput.BuildMode.SetCallbacks(this);
            _gameInput.EditMode.SetCallbacks(this);

            SetGamePlay();
            OnPauseEvent += SetUI;
            OnResumeEvent += SetGamePlay;
            OnBuildCancelEvent += CancelBuildMode;
            OnSelectCancelEvent += CancelEditMode;
        }

        private void SetGamePlay()
        {
            _gameInput.UI.Disable();
            _gameInput.GamePlay.Enable();
        }

        private void SetUI()
        {
            _gameInput.GamePlay.Disable();
            _gameInput.UI.Enable();
        }

        public void ActiveBuildMode()
        {
            isBuild = true;
            _gameInput.BuildMode.Enable();
        }

        private void CancelBuildMode()
        {
            isBuild = false;
            _gameInput.BuildMode.Disable();
        }

        public void ActiveEditMode()
        {
            isEdit = true;
            _gameInput.EditMode.Enable();
        }

        private void CancelEditMode()
        {
            isEdit = false;
            _gameInput.EditMode.Disable();
        }

        public void OnCameraMove(InputAction.CallbackContext context)
        {
            OnCameraMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraRotate(InputAction.CallbackContext context)
        {
            if (context.started)
                OnCameraRotateEvent?.Invoke(context.ReadValue<float>());
        }

        public void OnCursorPosition(InputAction.CallbackContext context)
        {
            OnCursorPositionEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (isBuild || isEdit) return;
                OnPauseEvent?.Invoke();
            }
        }

        public void OutOfMouse()
        {
            OnPauseEvent?.Invoke();
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnResumeEvent?.Invoke();
            }
        }

        public void OnBuildTower(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (UiManager.OnPointer) return;
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

        public void OnSelectTower(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (UiManager.OnPointer) return;
                OnSelectTowerEvent?.Invoke();
            }
        }

        public void OnSelectCancel(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnSelectCancelEvent?.Invoke();
            }
        }
    }
}