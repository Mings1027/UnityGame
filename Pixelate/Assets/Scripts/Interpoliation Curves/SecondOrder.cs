using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondOrder : MonoBehaviour
{
    public Transform target;
    public float speed, rotationSpeed;
    public float bounce;
    private Vector3 oldPos;

    private void FixedUpdate()
    {
        OrderSystem();
    }

    public void OrderSystem()
    {
        var curPos = transform.position;
        transform.position = Vector3.Lerp(transform.position, target.position, (1 - bounce) * speed * Time.fixedDeltaTime);
        transform.position = (1 + bounce) * transform.position - bounce * oldPos;
        oldPos = curPos;
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.fixedDeltaTime);

    }

}
