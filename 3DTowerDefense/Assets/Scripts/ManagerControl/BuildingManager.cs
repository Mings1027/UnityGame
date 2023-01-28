using System.Collections.Generic;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class BuildingManager : MonoBehaviour
    {
        private readonly List<GameObject> _towerList = new();
        [SerializeField] private string[] objects;
        private GameObject _pendingObject;
        public bool _isBuilding;
        public bool canPlace;

        //TowerButton Object OnClick Event
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


        public void TowerReset()
        {
            foreach (var t in _towerList)
            {
                t.SetActive(false);
            }
        }

        public void BuildTower(RaycastHit hit)
        {
            if (_isBuilding && canPlace)
            {
                print(hit.collider.name);
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

        public void TowerFollowMouse(Vector3 cursorPos)
        {
            if (_isBuilding)
                _pendingObject.transform.position =
                    cursorPos + new Vector3(0, _pendingObject.transform.localScale.y * 0.5f, 0);
        }
    }
}