using System;
using UnityEditor.UIElements;
using UnityEngine;

namespace IKControl
{
    public class IKController : MonoBehaviour
    {
        private Transform _rayCastOrigin;
        private RaycastHit _hit;
        [SerializeField] private LayerMask groundLayer;

        private void Awake()
        {
            _rayCastOrigin = transform.parent;
        }

        private void Update()
        {
            if (Physics.Raycast(_rayCastOrigin.position, Vector3.down, out _hit, groundLayer))
            {
                transform.position = _hit.point;
            }
        }
    }
}