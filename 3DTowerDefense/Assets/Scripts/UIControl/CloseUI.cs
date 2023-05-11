using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class CloseUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private UIManager _uiManager;

        private void Awake()
        {
            _uiManager = UIManager.Instance;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_uiManager.IsMoveUnit) return;
            _uiManager.CloseUI();
        }
    }
}
