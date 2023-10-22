using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        private InputManager _inputManager;
        
        [SerializeField] private TowerType towerType;
        [SerializeField] private bool isUnitTower;

        private void Awake()
        {
            _inputManager = FindObjectOfType<InputManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(towerType, isUnitTower);
        }
    }
}