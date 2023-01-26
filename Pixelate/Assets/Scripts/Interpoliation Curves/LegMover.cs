using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMover : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform target;
    public float distance;
    public float stepHeight;
    public float speed;
    public float lerpSpeed;
    public float rotationSpeed;

    public LegMover otherleg;

    private float lerp;
    private Vector3 oldPos, curPos, newPos;

    private void Awake()
    {
        oldPos = curPos = newPos = transform.position;
        lerp = 1;
    }

    private void FixedUpdate()
    {
        transform.position = curPos;
        if (Physics.Raycast(target.position, Vector3.down, out RaycastHit hit, 10, groundLayer))
        {
            if (Vector3.Distance(newPos, hit.point) > distance && !otherleg.IsMoving() && !IsMoving())
            {
                newPos = hit.point;
                lerp = 0;
            }
        }

        if (lerp < 1)
        {
            oldPos = transform.position;
            var centerPos = (oldPos + target.position) * 0.5f + Vector3.up * stepHeight;
            curPos = Vector3.Lerp(Vector3.Lerp(oldPos, centerPos, speed), Vector3.Lerp(centerPos, newPos, speed), lerp);
            lerp += Time.fixedDeltaTime * lerpSpeed;
        }
    }

    public bool IsMoving()
    {
        return lerp < 1;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(target.position, 0.1f);
    }

}
