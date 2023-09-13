using System;
using System.Collections.Generic;
using DG.Tweening;
using StatusControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    private Camera cam;
    private Vector3 lastPos;
    
    [SerializeField] private Grid grid;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private Transform mouseCursor;
    [SerializeField] private Transform cubeCursor;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        var mousePos = GetSelectedMapPos();
        var gridPos = grid.WorldToCell(mousePos);
        gridPos.y = 1;
        mouseCursor.position = mousePos;
        cubeCursor.position = grid.CellToWorld(gridPos);
    }

    private Vector3 GetSelectedMapPos()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = cam.nearClipPlane;
        var ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out var hit, 100, placementLayer))
        {
            lastPos = hit.point;
        }

        return lastPos;
    }
}