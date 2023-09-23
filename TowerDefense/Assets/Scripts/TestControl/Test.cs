using System;
using PoolObjectControl;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private string testString;
    [SerializeField] private string oldString;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // print(cam.ScreenToViewportPoint(Input.mousePosition));
        // print(cam.ViewportToScreenPoint(Input.mousePosition));
    }
}