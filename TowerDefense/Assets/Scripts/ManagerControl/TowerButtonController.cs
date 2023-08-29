using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        private string _towerName;

        [SerializeField] private InputManager inputManager;
        [SerializeField] private bool isUnitTower;

        // public event Action<bool> OnCheckTowerType;

        private void Awake()
        {
            _towerName = name.Replace("Button", "");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inputManager.enabled = true;
            inputManager.StartPlacement(_towerName, isUnitTower);
            // OnCheckTowerType?.Invoke(isUnitTower);
        }
    }
}