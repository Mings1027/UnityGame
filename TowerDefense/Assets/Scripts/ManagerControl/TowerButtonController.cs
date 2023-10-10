using CustomEnumControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class TowerButtonController : MonoBehaviour, IPointerDownHandler
    {
        private TowerManager _towerManager;
        [SerializeField] private TowerType towerType;
        [SerializeField] private bool isUnitTower;

        private void Awake()
        {
            _towerManager = GetComponentInParent<TowerManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _towerManager.inputManager.enabled = true;
            _towerManager.inputManager.StartPlacement(in towerType, isUnitTower);
        }
    }
}