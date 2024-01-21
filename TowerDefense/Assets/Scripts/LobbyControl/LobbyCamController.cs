using System;
using UnityEngine;

namespace GameControl
{
    public class LobbyCamController : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed;

        private void Awake()
        {
            Input.multiTouchEnabled = false;
        }

        private void LateUpdate()
        {
            transform.Rotate(Vector3.up * (rotateSpeed * Time.deltaTime));
        }
    }
}