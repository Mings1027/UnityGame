using UnityEngine;
using UnityEngine.InputSystem;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        public static bool IsPause { get; private set; }

        [SerializeField] private PlayerInput buildingManagerInput;
        [SerializeField] private GameObject menuPanel;

        public void OnMenuButton(InputAction.CallbackContext context)
        {
            if (context.started) //눌렀을때 정지 재생
            {
                IsPause = !IsPause;
                menuPanel.SetActive(IsPause);
                Time.timeScale = IsPause ? 0 : 1;
                buildingManagerInput.enabled = !IsPause; //정지면 마우스인풋 안받음
            }
        }
    }
}