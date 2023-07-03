using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    private NavMeshSurface _navMeshSurface;

    private void Awake()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        Debug.Log("editor");
#endif

#if UNITY_IOS
        Debug.Log("ios");
#endif
    }
}