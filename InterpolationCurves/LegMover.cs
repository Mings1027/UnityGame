using System;
using DG.Tweening;
using UnityEngine;

public class LegMover : MonoBehaviour
{
    public LayerMask groundLayer;
    public Transform target;
    public float distance;
    public float jumpPower;
    public float duration;
    private bool move;

    private Tweener moveTween;
    public Ease ease;
    private Vector3 targetVec;

    private void Awake()
    {
        moveTween = transform.DOMove(transform.position, 1).SetAutoKill(false);
    }

    private void Update()
    {
        if (Physics.Raycast(target.position, Vector3.down, out var hit, 10, groundLayer))
        {
            if (Vector3.Distance(target.position, hit.point) > distance)
            {
                targetVec = target.position;
                moveTween.ChangeEndValue(targetVec, duration, true).SetEase(ease).Restart();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(target.position, 0.1f);
    }
}