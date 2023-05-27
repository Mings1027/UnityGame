using System;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    public class InputManager : Singleton<InputManager>
    {
        private PlayerInput _playerInput;

        private InputAction _touchPositionAction;
        private InputAction _touchPressAction;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _touchPositionAction = _playerInput.actions["TouchPosition"];
            _touchPressAction = _playerInput.actions["TouchPress"];
        }

        private void OnEnable()
        {
            _touchPositionAction.started += TouchPosition;
            _touchPositionAction.performed += TouchPosition;
            _touchPressAction.performed += TouchPressed;
        }

        private void OnDisable()
        {
            _touchPositionAction.started -= TouchPosition;
            _touchPositionAction.performed -= TouchPosition;
            _touchPressAction.performed -= TouchPressed;
        }

        private void TouchPressed(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
        }

        private void TouchPosition(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            print(value);
        }
    }
}