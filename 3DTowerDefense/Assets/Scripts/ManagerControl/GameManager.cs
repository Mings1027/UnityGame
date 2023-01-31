using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private bool isPause;

        [SerializeField] private InputManager input;

        [SerializeField] private GameObject menuPanel;

        private void Awake()
        {
            input.OnPauseEvent += Pause;
            input.OnResumeEvent += Resume;
        }

        private void Pause()
        {
            isPause = true;
            menuPanel.SetActive(true);
            Time.timeScale = 0;
        }

        private void Resume()
        {
            isPause = false;
            menuPanel.SetActive(false);
            Time.timeScale = 1;
        }
    }
}