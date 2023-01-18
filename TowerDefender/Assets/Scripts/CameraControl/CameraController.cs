using System;
using PlayerControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraControl
{
    [Serializable]
    public class CameraController : MonoBehaviour
    {
        private Camera Cam { get; set; }

        private void Awake()
        {
            Cam = Camera.main;
        }

        private void Update()
        {
            GetMousePosition();
        }

        private void GetMousePosition()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            var mousePos = Mouse.current.position.ReadValue();
            Cam.ScreenToWorldPoint(mousePos);
            print(mousePos);
        }
    }
}