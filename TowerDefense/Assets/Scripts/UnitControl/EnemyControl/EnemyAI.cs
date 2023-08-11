using System;
using UnityEngine;
using Pathfinding;
using UnityEngine.Serialization;

public class EnemyAI : MonoBehaviour
{
    private Path _path;
    private Seeker _seeker;
    private Rigidbody _rigid;

    private int _curWayPoint;
    private bool _reachedEndOfPath;

    [SerializeField] private bool _canMove;

    public bool CanMove
    {
        get => _canMove;
        set => _canMove = value;
        // if (_canMove)
        //     UpdatePath();
    }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float nextWayPointDistance;
    [SerializeField] private float updatePathRepeatTime;
    
    private void OnEnable()
    {
        _canMove = true;
        _reachedEndOfPath = false;
        // UpdatePath();
        InvokeRepeating(nameof(UpdatePath), 0f, updatePathRepeatTime);
    }

    private void Start()
    {
        _seeker = GetComponent<Seeker>();
        _rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!CanMove) return;

        if (_path == null) return;

        if (_curWayPoint >= _path.vectorPath.Count)
        {
            _reachedEndOfPath = true;
            return;
        }

        Movement(out var direction);
        Rotation(direction);
        NextWayPoint();
    }

    private void OnDisable()
    {
        CancelInvoke();
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
        Quaternion rotation;
        if (direction != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rotation = _rigid.transform.rotation.normalized;
        }

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