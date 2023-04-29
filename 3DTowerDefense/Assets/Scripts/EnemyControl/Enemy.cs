using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemyControl
{
    public class Enemy : MonoBehaviour
    {
        private Rigidbody _rigid;

        private float _amount;
        private float _lerp;
        private int _interval;
        private Vector3 _point;

        private Vector3 _prevPos, _curPos, _nextPos;

        public Transform CurWayPoint;
        public int WayPointIndex;

        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private float moveSpeed;

        public event Action<Enemy> moveToNextWayPointEvent;

        private void Awake()
        {
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            WayPointIndex = 0;
        }

        private void OnDisable()
        {
            CancelInvoke();
            StackObjectPool.ReturnToPool(gameObject);
        }

        private void FixedUpdate()
        {
            // Move();
        }

        private void Update()
        {
            // CheckArrived();
            DistanceToWayPoint();
            BounceMove();
        }

        private void Move()
        {
            var pos = transform.position;
            var wayPos = CurWayPoint.position;

            var dir = (wayPos - pos).normalized;

            _rigid.MovePosition(_rigid.position + dir * (Time.fixedDeltaTime * moveSpeed));
            _rigid.MoveRotation(Quaternion.LookRotation(dir));
        }

        private void CheckArrived()
        {
            if (Vector3.Distance(transform.position, CurWayPoint.position) <= 0.2f)
            {
                moveToNextWayPointEvent?.Invoke(this);
            }
        }

        private void DistanceToWayPoint()
        {
            if (Vector3.Distance(transform.position, CurWayPoint.position) <= 0.2f)
            {
                moveToNextWayPointEvent?.Invoke(this);
                SetUp();
            }
        }

        private void BounceMove()
        {
            if (_lerp < 1)
            {
                _curPos = Vector3.Lerp(transform.position, _point, _lerp);
                _curPos.y += bounceCurve.Evaluate(_lerp);
                _lerp += Time.deltaTime * moveSpeed;
            }
            else
            {
                _lerp = 0;
                _nextPos = _point;
                _amount += _interval;
            }

            transform.position = _curPos;
        }

        public void SetUp()
        {
            _lerp = 1;
            var distance = Vector3.Distance(transform.position, CurWayPoint.position);
            _interval = 1 / Mathf.FloorToInt(distance);
            _point = Vector3.Lerp(transform.position, CurWayPoint.position, _amount + _interval);
        }
    }
}