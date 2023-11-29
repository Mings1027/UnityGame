using System;
using System.Collections;
using System.Collections.Generic;
using ManagerControl;
using TowerControl;
using UIControl;
using UnityEngine;

public class MoveUnitController : MonoBehaviour
{
    // private Camera cam;
    // private UnitTower _unitTower;
    // [SerializeField] private LayerMask groundLayer;
    //
    // private void Awake()
    // {
    //     cam = Camera.main;
    // }
    //
    // private void Update()
    // {
    //     if (Input.touchCount <= 0) return;
    //     CheckMoveUnit();
    // }
    //
    // private void CheckMoveUnit()
    // {
    //     var touch = Input.GetTouch(0);
    //     if (!touch.deltaPosition.Equals(Vector2.zero)) return;
    //     var ray = cam.ScreenPointToRay(Input.mousePosition);
    //     Physics.Raycast(ray, out var hit, groundLayer);
    //     if (hit.collider && Vector3.Distance(transform.position, hit.point) <= _unitTower.TowerRange)
    //     {
    //         TowerManager.Instance.StartMoveUnit(_unitTower, new Vector3(hit.point.x, 0, hit.point.z));
    //         UIManager.Instance.StartMoveUnit();
    //     }
    //     else
    //     {
    //         UIManager.Instance.YouCannotMove();
    //     }
    // }
}