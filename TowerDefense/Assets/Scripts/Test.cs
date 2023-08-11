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
    private RaycastHit hit;
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                print(hit.collider);
            }
        }

        print(Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hit.point, 1);
    }
}