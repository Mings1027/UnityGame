using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class test : MonoBehaviour
{
    private Rigidbody rigid;
    private Rigidbody rigid2;
    public GameObject t;

    private void Start()
    {
        t.GetComponent<Rigidbody>().AddForce(t.transform.forward * 10f, ForceMode.VelocityChange);
    }
}