using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Rigidbody rigid;

    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var pos = Vector3.MoveTowards(rigid.position, target.position, moveSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(pos);
    }
}