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
        private int pointerID;

        private Vector2 touchPos;

        public bool IsOnUI { get; set; }
        
        public event Action onClosePanelEvent;

        private void Awake()
        {
            
        }

        public void ClosePanelEvent()
        {
            onClosePanelEvent?.Invoke();
        }
    }
}