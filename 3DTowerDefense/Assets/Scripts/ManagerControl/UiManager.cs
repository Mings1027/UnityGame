using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class UiManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static bool IsPause { get; private set; }
        public bool OnPointer { get; private set; }

        private Transform _towerPos;

        [SerializeField] private PlayerInput playerManagerInput;
        [SerializeField] private GameObject menuPanel;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointer = true;
            Debug.Log("ON!");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointer = false;
            Debug.Log("OFF!");
        }

        public void OnMenuButton(InputAction.CallbackContext context)
        {
            if (context.started) //눌렀을때 정지 재생
            {
                IsPause = !IsPause;
                menuPanel.SetActive(IsPause);
                Time.timeScale = IsPause ? 0 : 1;
                playerManagerInput.enabled = !IsPause; //정지면 마우스인풋 안받음
            }
        }

    }
}