using UnityEngine;

public class IKController : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform legTarget;
    [SerializeField] private IKController otherLeg;

    [SerializeField] private float moveSpeed;
    [SerializeField] private AnimationCurve legCurve;
    [SerializeField] private float lerp;

    [SerializeField] private float radius;

    private Vector3 _oldPos, _curPos, _newPos;

    private RaycastHit _hit;

    private void Start()
    {
        lerp = 1;
        _oldPos = _curPos = _newPos = transform.position;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var r = Physics.Raycast(legTarget.position, Vector3.down, out _hit, 10, groundLayer);
        if (r)
        {
            if (Vector3.Distance(_newPos, _hit.point) > radius * 0.5f)
            {
                lerp = 0;
                _newPos = _hit.point;
                _oldPos = transform.position;
            }

            transform.position = _curPos;
        }

        if (lerp < 1)
        {
            _curPos = Vector3.Lerp(_oldPos, _newPos, lerp);
            _curPos.y = legCurve.Evaluate(lerp);
            lerp += Time.deltaTime * moveSpeed;
        }
    }

    private bool IsMoving() => lerp < 1;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_newPos, 0.1f);
        Gizmos.DrawWireSphere(_hit.point, radius);
    }
}