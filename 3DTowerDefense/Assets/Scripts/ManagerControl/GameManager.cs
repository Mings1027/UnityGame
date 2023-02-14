using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private bool isPause;

        [SerializeField] private InputController input;

        [SerializeField] private GameObject menuPanel;

        private void Awake()
        {
            input.OnPauseEvent += Pause;
            input.OnResumeEvent += Resume;
        }

        public void Pause()
        {
            isPause = true;
            menuPanel.SetActive(true);
            Time.timeScale = 0;
        }

        public void Resume()
        {
            isPause = false;
            menuPanel.SetActive(false);
            Time.timeScale = 1;
            input.Resume();
        }
    }
}