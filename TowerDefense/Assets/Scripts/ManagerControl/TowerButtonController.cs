using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private TowerType towerType;
        [SerializeField] private bool isUnitTower;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            InputManager.Instance.enabled = true;
            InputManager.Instance.StartPlacement(in towerType, isUnitTower);
        }
    }
}