using CustomEnumControl;
using DataControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ManagerControl
{
    public class
        TowerButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
            IBeginDragHandler, IDragHandler
    {
        private InputManager _inputManager;

        [SerializeField] private TowerData towerData;
        [SerializeField] private TowerInfoUI towerInfoUI;
        [SerializeField] private bool isUnitTower;

        private void Awake()
        {
            _inputManager = FindObjectOfType<InputManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(towerData.TowerType, isUnitTower);
            towerInfoUI.SetPanelInfo(towerData, isUnitTower);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            towerInfoUI.DisablePanel();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            towerInfoUI.DisablePanel();
        }

        public void OnDrag(PointerEventData eventData)
        {
        }
    }
}