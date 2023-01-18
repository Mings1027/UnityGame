using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKMover : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform target;
    [SerializeField] private float distance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private AnimationCurve legCurve;
    [SerializeField] private float lerp;
    [SerializeField] private IKMover otherLeg;

    private Vector3 oldPos, curPos, newPos;

    private void Awake()
    {
        lerp = 1;
        oldPos = curPos = newPos = transform.position;
    }


    private void Update()
    {
        transform.position = curPos;
        if (Physics.Raycast(target.position, Vector3.down, out var hit, 10f, groundLayer))
        {
            if (Vector3.Distance(newPos, hit.point) > distance && !IsMoving)
            {
                newPos = hit.point;
                lerp = 0;
                oldPos = transform.position;
            }
        }

        if (IsMoving)
        {
            curPos = Vector3.Lerp(oldPos, newPos, lerp);
            curPos.y = legCurve.Evaluate(lerp);
            lerp += Time.deltaTime * moveSpeed;
        }
    }

    public bool IsMoving => lerp < 1;

    // public bool GroundCheck()
    // {
    //     return Physics.Raycast(transform.position, Vector3.down, out var hit, 0.3f, groundLayer)
    //     ? true : false;
    // }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        var position = target.position;
        Gizmos.DrawSphere(position, 0.1f);
    }
}