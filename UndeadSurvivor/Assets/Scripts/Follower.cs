using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform floatingObj;
    [SerializeField] private float bounceSpeed, speed;
    [SerializeField] [Range(1, 50)] private int followDelay;
    private Queue<Vector3> _followPosQueue;
    private Vector3 _curPos;

    private void Start()
    {
        _followPosQueue = new Queue<Vector3>();
    }

    public void Update()
    {
        QueueFollow();
        FloatingObj();
    }

    private void QueueFollow()
    {
        _curPos = transform.position;
        if (!_followPosQueue.Contains(target.position))
            _followPosQueue.Enqueue(target.position);

        if (_followPosQueue.Count > followDelay)
        {
            _curPos = _followPosQueue.Dequeue();
        }
        else if (_followPosQueue.Count < followDelay)
        {
            _curPos = target.position;
        }

        transform.position = _curPos;
    }

    private void FloatingObj()
    {
        floatingObj.position = new Vector3(
            _curPos.x,
            _curPos.y + Mathf.Sin(Time.time * speed) * bounceSpeed,
            _curPos.z);
    }
}