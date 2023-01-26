using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hello : MonoBehaviour
{
    public Transform a, b;
    public float speed;

    private void Update()
    {
        speed = (speed + Time.deltaTime) % 1f;
        print(speed);
        transform.position = Vector3.Slerp(a.position, b.position, speed);
    }
}