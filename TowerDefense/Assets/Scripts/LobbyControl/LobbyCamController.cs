using System;
using UnityEngine;

namespace LobbyControl
{
    public class LobbyCamController : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;

        private void Start()
        {
            Input.multiTouchEnabled = false;
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
        }

        private void LateUpdate()
        {
            transform.Rotate(Vector3.up * (rotateSpeed * Time.deltaTime));
        }
    }
}