using System;
using Pathfinding;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyAI : MonoBehaviour
    {
        private Path _path;
        private Seeker _seeker;
        private Rigidbody _rigid;

        private byte _curWayPoint;

        public bool CanMove { get; set; }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        [SerializeField] private float moveSpeed;
        [SerializeField] private float nextWayPointDistance;
        [SerializeField] private float updatePathRepeatTime;

        private void Awake()
        {
            _seeker = GetComponent<Seeker>();
            _rigid = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            CanMove = true;
            UpdatePath();
            // InvokeRepeating(nameof(UpdatePath), 0f, updatePathRepeatTime);
        }

        private void FixedUpdate()
        {
            if (!CanMove) return;

            if (_path.Equals(null)) return;

            Movement(out var direction);
            Rotation(direction);
            NextWayPoint();
        }

        private void OnDisable()
        {
            CancelInvoke();
            _curWayPoint = 0;
        }

        private void Movement(out Vector3 direction)
        {
            var position = _rigid.position;
            direction = (_path.vectorPath[_curWayPoint] - position).normalized;
            var force = direction * (moveSpeed * Time.deltaTime);

            _rigid.MovePosition(position + force);
        }

        private void Rotation(Vector3 direction)
        {
            var rotation = direction != Vector3.zero
                ? Quaternion.LookRotation(direction, Vector3.up)
                : _rigid.rotation.normalized;

            _rigid.MoveRotation(rotation);
        }

        private void NextWayPoint()
        {
            var distance = Vector3.Distance(_rigid.position, _path.vectorPath[_curWayPoint]);

            if (distance < nextWayPointDistance)
            {
                _curWayPoint++;
            }
        }

        private void UpdatePath()
        {
            if (_seeker.IsDone())
            {
                _seeker.StartPath(_rigid.position, Vector3.zero, OnPathComplete);
            }
        }

        private void OnPathComplete(Path p)
        {
            if (p.error) return;
            _path = p;
            _curWayPoint = 0;
        }
    }
}