using System;
using GameControl;
using TowerDefenseInput;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace ManagerControl
{
    public class InputManager : Singleton<InputManager>
    {
        private TouchControls touchControls;
        private TowerPlacementManager towerPlacementManager;
        private Vector2 touchPos;
        public event Action onClosePanelEvent;

        private void Awake()
        {
            touchControls = new TouchControls();
            towerPlacementManager = TowerPlacementManager.Instance;
        }

        private void OnEnable()
        {
            touchControls.Enable();
        }

        private void OnDisable()
        {
            touchControls.Disable();
        }

        private void Start()
        {
            touchControls.GamePlay.TouchPress.canceled += PressScreen;
            touchControls.GamePlay.TouchPosition.started += TouchScreenPosition;
        }

        private void PressScreen(InputAction.CallbackContext ctx)
        {
            
        }

        private void TouchScreenPosition(InputAction.CallbackContext ctx)
        {
            touchPos = ctx.ReadValue<Vector2>();
        }
    }
}