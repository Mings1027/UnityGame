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

        private Vector3 _prevWayPoint;
        private Vector3 _curWayPoint;
        private int _wayPointIndex;
        private bool _startMove;

        [SerializeField] private AnimationCurve jumpCurve;
        [SerializeField] private float moveSpeed;

        public event Action<Enemy> moveToNextWayPointEvent;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _startMove = false;
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
            moveToNextWayPointEvent = null;
        }

        private void FixedUpdate()
        {
            if (!_startMove) return;
            Move();
        }

        private void Update()
        {
            if (!_startMove) return;
            CheckGround();
            CheckArrived();
        }

        private void LookWayPoint()
        {
            _dir = (_curWayPoint - _prevWayPoint).normalized;
            _rigid.transform.forward = _dir;
        }

        private void Move()
        {
            if (_lerp > 1) return;
            _lerp += Time.deltaTime * moveSpeed;
            _curPos = Vector3.Lerp(_prevPos, _nextPos, _lerp);
            _curPos.y += jumpCurve.Evaluate(_lerp);
            _rigid.position = _curPos;
        }

        private void CheckGround()
        {
            if (_lerp <= 1) return;
            {
                _prevPos = _rigid.position;
                _nextPos = _prevPos + _dir;
                _lerp = 0;
            }
        }

        private void CheckArrived()
        {
            if (Vector3.Distance(_rigid.position, _curWayPoint) <= 0.5f)
            {
                moveToNextWayPointEvent?.Invoke(this);
                LookWayPoint();
            }
        }

        public void Init(bool startMove,Vector3 firstWayPoint, Vector3 secondWayPoint)
        {
            _startMove = startMove;
            _prevPos = _curPos = _nextPos = firstWayPoint;
            _lerp = 0;
            _wayPointIndex = 0;
            _prevWayPoint = firstWayPoint;
            _curWayPoint = secondWayPoint;
            LookWayPoint();
        }

        public void SetDestination(Transform[] waypoint)
        {
            _prevWayPoint = waypoint[_wayPointIndex].position;
            if (waypoint[_wayPointIndex++] == waypoint[^1])
            {
                gameObject.SetActive(false);
                return;
            }

            _curWayPoint = waypoint[_wayPointIndex].position;
        }
    }
}