using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerSpawner : MonoBehaviour
{
    private Ray _ray;
    private Camera _cam;
    private Vector3 _mousePos;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnMousePosition(InputValue value)
    {
        _mousePos = value.Get<Vector2>();
    }

    private void OnClick(InputValue value)
    {
        if (value.Get<float>() <= 0) return;
        _ray = _cam.ScreenPointToRay(_mousePos);
        if (Physics.Raycast(_ray, out var hit, Mathf.Infinity))
        {
            if (hit.transform.CompareTag("Tower"))
            {
                print("Already Placed Tower!");
            }
            else if (hit.transform.CompareTag("Ground"))
            {
                var position = hit.transform.position;
                var t = StackObjectPool.Get("Tower", position);
                t.transform.position = position + new Vector3(0, hit.transform.localScale.y * 0.5f, 0) +
                                       new Vector3(0, t.transform.localScale.y * 0.5f, 0);
            }
        }
    }
}