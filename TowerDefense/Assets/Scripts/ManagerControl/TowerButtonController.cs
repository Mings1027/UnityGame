using System;
using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        private GameManager _gameManager;
        
        [SerializeField] private TowerType towerType;
        [SerializeField] private bool isUnitTower;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _gameManager.inputManager.enabled = true;
            _gameManager.inputManager.StartPlacement(towerType, isUnitTower);
        }
    }
}