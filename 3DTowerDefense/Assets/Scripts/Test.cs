using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Test :MonoBehaviour
{
    [SerializeField] private Transform weapon;
    [SerializeField] private int radius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Collider[] hitCollider;

    private void Awake()
    {
        hitCollider = new Collider[5];
    }

    private void Update()
    {
        var size = Physics.OverlapSphereNonAlloc(weapon.position, radius, hitCollider, enemyLayer);
        for (int i = 0; i < size; i++)
        {
            print(hitCollider[i].name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(weapon.position,radius);
    }
}