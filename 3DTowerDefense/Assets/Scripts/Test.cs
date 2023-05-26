using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private Rigidbody _rigid;

    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var pos = Vector3.MoveTowards(_rigid.position, target.position, moveSpeed * Time.fixedDeltaTime);
        _rigid.MovePosition(pos);
    }
}