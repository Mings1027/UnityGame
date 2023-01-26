using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private InputActionReference rotQ, rotE;
    [SerializeField] private Transform target;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private float rotateAngle;
    private Quaternion rotY;

    [SerializeField] private float smoothSpeed;

    private void Start()
    {
        rotY = transform.rotation;
    }

    private void Update()
    {
        SmoothMove();
        Rotate();
    }
    private void SmoothMove()
    {
        var oldPos = transform.position;
        var curPos = Vector3.Lerp(oldPos, target.position, smoothSpeed);
        transform.position = curPos;
    }
    private void Rotate()
    {
        if (rotQ.action.triggered) rotY *= Quaternion.AngleAxis(rotateAngle, Vector3.up);
        if (rotE.action.triggered) rotY *= Quaternion.AngleAxis(-rotateAngle, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotY, rotateSpeed * Time.deltaTime);
    }
}
