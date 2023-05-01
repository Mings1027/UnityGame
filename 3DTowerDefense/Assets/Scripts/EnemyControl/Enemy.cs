using System;
using GameControl;
using UnityEngine;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        private Rigidbody _rigid;
        private float _lerp;
        private Vector3 _prevPos, _curPos, _nextPos;
        private Vector3 _dir;

        public Vector3 PrevWayPoint { get; set; }
        public Vector3 CurWayPoint { get; set; }
        public int WayPointIndex { get; set; }

        [SerializeField] private AnimationCurve jumpCurve;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float moveSpeed;

        public event Action<Enemy> moveToNextWayPointEvent;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            moveToNextWayPointEvent = null;
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Update()
        {
            CheckGround();
            CheckArrived();
        }

        private void LookWayPoint()
        {
            _dir = (CurWayPoint - PrevWayPoint).normalized;
            _rigid.transform.forward = _dir;
        }

        public void Init(Vector3 firstWayPoint, Vector3 secondWayPoint)
        {
            _prevPos = _curPos = _nextPos = _rigid.position;
            _lerp = 0;
            WayPointIndex = 0;
            PrevWayPoint = firstWayPoint;
            CurWayPoint = secondWayPoint;
            LookWayPoint();
        }

        private void Move()
        {
            if (_lerp <= 1)
            {
                print("move");
                _lerp += Time.deltaTime * moveSpeed;
                _curPos = Vector3.Lerp(_prevPos, _nextPos, _lerp);
                _curPos.y += jumpCurve.Evaluate(_lerp);
                _rigid.position = _curPos;
            }
        }

        private void CheckGround()
        {
            if (_lerp > 1)
            {
                print("check Ground");
                Debug.DrawRay(transform.position + _dir + Vector3.up, Vector3.down);
                if (Physics.Raycast(_rigid.position + _dir + Vector3.up, Vector3.down, 10, groundLayer))
                {
                    _prevPos = _rigid.position;
                    _nextPos = _prevPos + _dir;
                    _lerp = 0;
                }
            }
        }

        private void CheckArrived()
        {
            if (Vector3.Distance(_rigid.position, CurWayPoint) <= 0.5f)
            {
                print("check Arrived");
                moveToNextWayPointEvent?.Invoke(this);
                LookWayPoint();
            }
        }
    }
}