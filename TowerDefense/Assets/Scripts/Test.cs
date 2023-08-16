using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour
{
    private Camera cam;

    [SerializeField] private List<Vector3> testList;

    private void Awake()
    {
        cam = Camera.main;
        testList = new List<Vector3>();
    }

    private void Start()
    {
        testList.Add(Vector3.up);
        testList.Add(Vector3.right);
        testList.Add(Vector3.left);
        testList.Add(Vector3.down);

        testList.Remove(Vector3.right);
    }

    

    private void OnDrawGizmos()
    {
        
    }
}