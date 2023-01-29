using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
   [SerializeField] private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        var materialColor = _meshRenderer.material.color;
        materialColor.a = 0;
        _meshRenderer.material.color = materialColor;
    }
}