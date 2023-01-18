using System;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerSpawner : MonoBehaviour
{
    private Camera cam;
    private Vector2 mousePos;
    private Ray ray;
    private RaycastHit hit;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnMousePosition(InputValue value)
    {
        mousePos = value.Get<Vector2>();
    }

    private void OnLeftClick(InputValue value)
    {
        if (value.Get<float>() <= 0) return;
        Spawn();
    }

    private void Spawn()
    {
        ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.CompareTag("Tower")) return;
            StackObjectPool.Get("Tower", hit.transform.position);
        }
    }

    private void SpawnTower(Component tileTransform)
    {
        // if (tileTransform.TryGetComponent<Tower>(out _)) return;
        StackObjectPool.Get("Tower", hit.transform.position);
    }
}