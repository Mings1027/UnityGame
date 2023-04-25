using System;
using System.Collections.Generic;
using GameControl;
using TMPro;
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
        private Camera cam;
        private EventSystem eventSystem;
        private PlayerInput playerInput;

        private InputAction touchPositionAction, touchClickAction;

        private Vector2 touchPos;

        public bool IsOnUI { get; set; }
        public TextMeshProUGUI t;
        public event Action onClosePanelEvent;

        private void Awake()
        {
            cam = Camera.main;
            eventSystem = EventSystem.current;
            playerInput = GetComponent<PlayerInput>();
            touchClickAction = playerInput.actions["TouchClick"];
            touchPositionAction = playerInput.actions["TouchPosition"];
            
        }

        // private void OnEnable()
        // {
        //     touchClickAction.performed += TouchClick;
        //     touchPositionAction.performed += TouchScreenPosition;
        // }
        //
        // private void OnDisable()
        // {
        //     touchClickAction.performed -= TouchClick;
        //     touchPositionAction.performed -= TouchScreenPosition;
        // }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                if (eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    t.text = "UI";
                }
                else
                {
                    t.text = "not UI";
                }
            }
        }

        private bool IsPointerOverUIObject()
        {
            var eventDataCurrentPosition = new PointerEventData(eventSystem);
            eventDataCurrentPosition.position = new Vector2(touchPos.x, touchPos.y);
            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(eventDataCurrentPosition,results);
            return results.Count > 0;
        }

        private void TouchClick(InputAction.CallbackContext context)
        {
            touchPos = touchPositionAction.ReadValue<Vector2>();
            var ray = cam.ScreenPointToRay(touchPos);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    onClosePanelEvent?.Invoke();
                }
            }
        }

        private void TouchScreenPosition(InputAction.CallbackContext ctx)
        {
            touchPos = ctx.ReadValue<Vector2>();
        }
    }
}