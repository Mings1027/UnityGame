using System.Collections.Generic;
using GameControl;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    public class BuildingManager : MonoBehaviour
    {
        private Camera _cam;
        private readonly List<GameObject> _towerList = new();
        [SerializeField] private string[] objects;
        private GameObject _pendingObject;
        public bool _isBuilding;
        public bool canPlace;

        private Ray _camRay;
        private Vector3 _cursorPos;
        private RaycastHit _hit;

        [SerializeField] private LayerMask cursorMoveLayer;

        [SerializeField] private Transform cursorObj;

        private void Awake()
        {
            _cam = Camera.main;
        }

        public void OnMoveCursorPosition(InputAction.CallbackContext context)
        {
            _camRay = _cam.ScreenPointToRay(context.ReadValue<Vector2>());

            MoveCursor();
            TowerFollowMouse(_cursorPos);
        }

        public void OnBuild(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                BuildTower(_hit);
            }
        }

        private void MoveCursor()
        {
            if (Physics.Raycast(_camRay, out _hit, Mathf.Infinity, cursorMoveLayer))
            {
                _cursorPos.x = Mathf.Round(_hit.point.x);
                _cursorPos.y = Mathf.Round(_hit.point.y);
                _cursorPos.z = Mathf.Round(_hit.point.z);

                cursorObj.position = _cursorPos;
            }
        }

        private void TowerFollowMouse(Vector3 cursorPos)
        {
            if (_isBuilding)
            {
                _pendingObject.transform.position =
                    cursorPos + new Vector3(0, _pendingObject.transform.localScale.y * 0.5f, 0);
            }
        }

        private void BuildTower(RaycastHit hit)
        {
            if (_isBuilding && canPlace)
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    _isBuilding = false;
                    _pendingObject = null;
                }
                else
                {
                    print("Wrong Place");
                }
            }
        }

        //OnClick Event Tower Button Object
        public void SelectedObject(int index)
        {
            if (_pendingObject != null)
            {
                var obj = _pendingObject;
                obj.SetActive(false);
            }
            else
            {
                _isBuilding = true;
            }

            _pendingObject = StackObjectPool.Get(objects[index], transform.position);
            _towerList.Add(_pendingObject);
        }

        //OnClick Event GenerateNewMap Button Object
        public void TowerReset()
        {
            foreach (var t in _towerList)
            {
                t.SetActive(false);
            }
        }
    }
}