using DG.Tweening;
using UnityEngine;

public class IKMover : MonoBehaviour
{
    private Vector3 _prevPos, _curPos, _nextPos;
    private float _lerp;

    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform sensor;
    [SerializeField] private AnimationCurve legCurve;
    [SerializeField] [Range(0, 10)] private float radius;


    private void Start()
    {
        _lerp = 1;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Physics.Raycast(sensor.position, Vector3.down, out var _hit, 100, groundLayer))
        {
            if (Vector3.Distance(_nextPos, _hit.point) > radius)
            {
                _lerp = 0;
                _nextPos = _hit.point;
                _prevPos = transform.position;
            }
            else
            {
                transform.position = _curPos;
            }
        }

        if (_lerp < 1)
        {
            _curPos = Vector3.Lerp(_prevPos, _nextPos, _lerp);
            _curPos.y = legCurve.Evaluate(_lerp);
            _lerp += Time.deltaTime * moveSpeed;
        }
        else
        {
            _prevPos = _nextPos;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(_prevPos, 0.1f);
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(_curPos, 0.1f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_nextPos, 0.1f);
        Gizmos.DrawRay(sensor.position, Vector3.down);
    }
}