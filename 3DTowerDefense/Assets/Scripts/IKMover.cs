using UnityEngine;

public class IKMover : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform legSensor;
    [SerializeField] private Transform rigTarget;

    [SerializeField] private float moveSpeed;
    [SerializeField] private AnimationCurve legCurve;
    [SerializeField] private float lerp;

    [SerializeField] private float radius;

    private Vector3 _oldPos, _curPos, _newPos;

    private RaycastHit _hit;

    private void Start()
    {
        lerp = 1;
        _oldPos = _curPos = _newPos = legSensor.position;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var r = Physics.Raycast(legSensor.position, Vector3.down, out _hit, 10, groundLayer);
        if (r)
        {
            if (Vector3.Distance(_newPos, _hit.point) > radius)
            {
                _oldPos = rigTarget.position;
                _newPos = _hit.point;
                lerp = 0;
            }
            else
            {
                rigTarget.position = _newPos;
            }
        }

        if (lerp < 1) LegMove();
    }

    private void LegMove()
    {
        if (lerp >= 1) return;
        _curPos = Vector3.Lerp(_oldPos, _newPos, lerp);
        _curPos.y = legCurve.Evaluate(lerp);
        lerp += Time.deltaTime * moveSpeed;
        rigTarget.position = _curPos;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_oldPos, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_curPos, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_newPos, 0.1f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(_hit.point, radius * 2);
    }
}