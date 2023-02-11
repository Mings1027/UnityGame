using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace IKControl
{
    public class SpiderIKController : MonoBehaviour
    {
        private RaycastHit[] _hit;
        private Vector3 oldBodyPos, curBodyPos, nextBodyPos;
        private Vector3[] _oldLegPos, _curLegPos, _nextLegPos;
        private float _lerp;
        private bool[] _isMoving;
        private int _legIndex;

        [SerializeField] [Range(0, 1)] private float bounce;
        [SerializeField] private float speed, rotationSpeed;
        [SerializeField] private Transform body;

        [SerializeField] private LayerMask walkableLayer;
        [SerializeField] private Transform[] legs;

        [FormerlySerializedAs("_sensor")] [SerializeField]
        private Transform[] sensor;

        [SerializeField] private float stepDistance;
        [SerializeField] private float stepTime;
        [SerializeField] private float stepHeight;


        private void Start()
        {
            _hit = new RaycastHit[legs.Length];
            _oldLegPos = _curLegPos = _nextLegPos = new Vector3[legs.Length];
            for (int i = 0; i < legs.Length; i++)
            {
                _oldLegPos[i] = _curLegPos[i] = _nextLegPos[i] = legs[i].position;
            }

            // _lerp = new float[legs.Length];
            _isMoving = new bool[legs.Length];
        }

        private void FixedUpdate()
        {
            Check();
            BodyMove();
        }


        private void Check()
        {
            for (var i = 0; i < legs.Length; i++)
            {
                legs[i].position = _curLegPos[i];
            }

            if (Physics.Raycast(sensor[_legIndex].position, Vector3.down, out _hit[_legIndex], 100, walkableLayer))
            {
                if (Vector3.Distance(_nextLegPos[_legIndex], _hit[_legIndex].point) > stepDistance)
                {
                    _lerp = 0;
                    _oldLegPos[_legIndex] = legs[_legIndex].position;
                    _nextLegPos[_legIndex] = _hit[_legIndex].point;
                }
            }

            if (_lerp < 1)
            {
                _curLegPos[_legIndex] = Vector3.Lerp(_oldLegPos[_legIndex], _nextLegPos[_legIndex], _lerp);
                _lerp += Time.fixedDeltaTime / stepTime;

                // if (body.position != transform.position)
                //     _curLegPos[_legIndex].y += Mathf.Sin(_lerp * Mathf.PI * stepHeight);
            }

            if (_lerp >= 1)
                _legIndex = (_legIndex + 1) % legs.Length;
        }


        private void BodyMove()
        {
            var bodyPos = body.position;
            curBodyPos = Vector3.Lerp(bodyPos, transform.position, (1 - bounce) * speed * Time.fixedDeltaTime);
            curBodyPos += bounce * (curBodyPos - oldBodyPos);
            oldBodyPos = bodyPos;
            body.SetPositionAndRotation(curBodyPos,
                Quaternion.Slerp(body.rotation, transform.rotation,
                    (1 - bounce) * rotationSpeed * Time.fixedDeltaTime));
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (Application.isPlaying)
            {
                for (var i = 0; i < legs.Length; i++)
                {
                    Gizmos.DrawSphere(_nextLegPos[i], 0.1f);
                }
            }

            Gizmos.color = Color.green;
            for (var i = 0; i < legs.Length; i++)
            {
                Gizmos.DrawSphere(_hit[i].point, 0.1f);
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < legs.Length; i++)
            {
                Gizmos.DrawLine(body.position, legs[i].position);
            }
        }
    }
}