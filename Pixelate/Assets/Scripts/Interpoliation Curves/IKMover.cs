using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class IKMover : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform target;
    public float distance, stepHeight;

    public float lerpTime;

    public bool isMove;
    private Vector3 oldPos, curPos, newPos;

    private void Awake()
    {
        oldPos = curPos = newPos = transform.position;
    }

    public void CheckMove()
    {
        if (isMove) return;

        if (Physics.Raycast(target.position, Vector3.down, out RaycastHit hit, 10, groundLayer))
        {
            if (Vector3.Distance(newPos, target.position) > distance)
            {
                newPos = hit.point;
                StartCoroutine(LegMove());
            }
        }
    }

    private IEnumerator LegMove()
    {
        isMove = true;

        transform.position = curPos;
        oldPos = transform.position;
        var centerPos = (oldPos + target.position) * 0.5f + Vector3.up * stepHeight;

        float curTime = 0;
        do
        {
            curTime += Time.deltaTime;
            var normalizeTime = curTime / lerpTime;

            curPos = Vector3.Lerp(
                    Vector3.Lerp(oldPos, centerPos, normalizeTime),
                    Vector3.Lerp(centerPos, newPos, normalizeTime), normalizeTime);
            yield return null;

        } while (curTime < lerpTime);

        isMove = false;
    }

}
