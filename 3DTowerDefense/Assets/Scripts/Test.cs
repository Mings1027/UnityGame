using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Collider[] targets;
    [SerializeField] private LayerMask enemylayer;

    private void Update()
    {
        var size = Physics.OverlapSphereNonAlloc(transform.position, 2, targets, enemylayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 2);
    }
}