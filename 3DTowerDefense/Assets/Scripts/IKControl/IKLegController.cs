using System;
using Unity.VisualScripting;
using UnityEngine;

namespace IKControl
{
    public class IKLegController : MonoBehaviour
    {
        private RaycastHit _hit;
        private float _shortestDistance = float.MaxValue;
        private Transform _shortestLegFromBody;
        private Vector3 _prevPos, _curPos, _nextPos;
        private float _lerp;

        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform[] legs;
        [SerializeField] private Transform[] sensor;
        [SerializeField] private float radius;

        private void Start()
        {
            _lerp = 1;
            _prevPos = _curPos = _nextPos = legs[0].position;
        }

        private void Update()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            if (Physics.Raycast(legs[0].position, Vector3.down, out _hit, 100, groundLayer))
            {
                if (Vector3.Distance(_nextPos, _hit.point) > radius)
                {
                    _lerp = 0;
                    _nextPos = _hit.point;
                    _prevPos = legs[0].position;
                }
                else
                {
                    legs[0].position = _curPos;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_hit.point,0.1f);
        }
    }
}