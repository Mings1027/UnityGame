using UnityEngine;
using UnityEngine.InputSystem;

namespace GameControl
{
    public class GameManager : MonoBehaviour
    {
        private static bool isPause;
        public static bool IsPause => isPause;

        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private GameObject menuPanel;

        public void OnMenuButton(InputAction.CallbackContext context)
        {
            if (context.started)    //눌렀을때 정지 재생
            {
                isPause = !isPause;
                menuPanel.SetActive(isPause);
                Time.timeScale = isPause ? 0 : 1;
                _playerInput.enabled = !isPause;    //정지면 마우스인풋 안받음
            }
        }
    }
}