using System;
using UnityEngine;


public class TestTower : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField]private Vector3 moveVec;

    [SerializeField] private float speed;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rigid.MovePosition(rigid.position + moveVec * (speed * Time.fixedDeltaTime));
    }
}