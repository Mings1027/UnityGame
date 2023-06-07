using System;
using TowerControl;
using UnityEngine;

namespace UnitControl
{
    public class MoveUnitIndicator : MonoBehaviour
    {
        private Camera _cam;
        public event Action onMoveUnitEvent;
        public BarracksUnitTower BarracksTower { get; set; }

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount <= 0) return;
            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended) return;
            
            var ray = _cam.ScreenPointToRay(touch.position);
            
            if (!Physics.Raycast(ray, out var hit)) return;
            if (BarracksTower.UnitMove(hit.point))
            {
                onMoveUnitEvent?.Invoke();
                gameObject.SetActive(false);
            }
            else
            {
                print("cant move");
            }
        }
    }
}