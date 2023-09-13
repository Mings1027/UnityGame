using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        private string _towerName;

        [SerializeField] private bool isUnitTower;

        private void Awake()
        {
            _towerName = name.Replace("Button", "");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            InputManager.Instance.enabled = true;
            InputManager.Instance.StartPlacement(_towerName, isUnitTower);
        }
    }
}