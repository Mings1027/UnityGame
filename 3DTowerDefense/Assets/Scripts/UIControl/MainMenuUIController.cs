using System;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        [SerializeField] private Transform startPanel;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            startPanel.DOMoveY(Screen.width * 2, 2).SetEase(Ease.OutBack);
        }
    }
}