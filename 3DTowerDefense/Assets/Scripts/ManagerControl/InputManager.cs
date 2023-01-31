using System;
using TowerDefenseInput;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    [CreateAssetMenu(menuName = "InputManager")]
    public class InputManager : ScriptableObject, GameInput.IGamePlayActions, GameInput.IUIActions
    {
        private GameInput _gameInput;

        public event Action<Vector2> OnCameraMoveEvent;
        public event Action<float> OnCameraRotateEvent;

        public event Action<Vector2> OnCursorPositionEvent;

        public event Action OnActiveBuildModeEvent;

        public event Action OnPauseEvent, OnResumeEvent;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.GamePlay.SetCallbacks(this);
                _gameInput.UI.SetCallbacks(this);

                SetGamePlay();
            }
        }

        private void SetGamePlay()
        {
            _gameInput.GamePlay.Enable();
            _gameInput.UI.Disable();
        }

        private void SetUI()
        {
            _gameInput.GamePlay.Disable();
            _gameInput.UI.Enable();
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

        public void OnBuildMode(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnActiveBuildModeEvent?.Invoke();
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnPauseEvent?.Invoke();
                SetUI();
            }
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                OnResumeEvent?.Invoke();
                SetGamePlay();
            }
        }
    }
}