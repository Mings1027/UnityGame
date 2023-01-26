using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;

namespace GameControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference manu;

        [SerializeField] private UnityEvent onPause;

        [SerializeField] private EventSystem eventSystem;

        [SerializeField] private GameObject manuPanel, optionPanel;
        [SerializeField] private GameObject optionButton, titleButton;

        private readonly Stack<GameObject> panelStack = new(), buttonStack = new();

        private bool pause;

        private void Awake()
        {
            eventSystem.SetSelectedGameObject(optionButton);
            panelStack.Push(manuPanel);
        }

        private void Update()
        {
            if (!manu.action.triggered) return;
            if (panelStack.Count == 1)
            {
                onPause?.Invoke();
                pause = !pause;
                Time.timeScale = pause ? 0 : 1;
                panelStack.Peek().SetActive(pause);
            }
            else
            {
                panelStack.Pop().SetActive(false);
                panelStack.Peek().SetActive(true);
            }
        }

        public void Option()
        {
            PanelManager.PanelControl(panelStack, optionPanel);
        }

        public void Title()
        {
            SceneManager.LoadScene("ReadyScene");
            Time.timeScale = 1;
        }
    }
}