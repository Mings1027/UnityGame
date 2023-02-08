using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace InterpolationCurves
{
    public class FirstOrder : MonoBehaviour
    {
        public float lerp;
        private Vector3 prevPos, curPos, nextPos;
        private RaycastHit _hit;
        private Sequence _legSequence;

        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform leg;
        [SerializeField] private Transform sensor;
        [SerializeField] private float radius;
        [SerializeField] private float jumpPower;
        [SerializeField] private float duration;

        private void Update()
        {
            if (Physics.Raycast(sensor.position, Vector3.down, out _hit, 100, groundLayer))
            {
                if (Vector3.Distance(nextPos, _hit.point) > radius)
                {
                    nextPos = _hit.point;
                    prevPos = leg.position;
                    leg.DOJump(nextPos, jumpPower, 1, duration);
                }

                leg.position = prevPos;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(nextPos, 0.1f);
        }
    }
}