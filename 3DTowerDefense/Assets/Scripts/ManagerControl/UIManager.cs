using System;
using BuildControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using TMPro;
using ToolTipControl;
using TowerControl;
using UIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManagerControl
{
    public sealed class UIManager : Singleton<UIManager>
    {
        private bool IsPause { get; set; }
        private GameObject _menuPanel;
        private GameObject _pauseButton;

        [SerializeField] private GameObject startPanel;
        [SerializeField] private Button startButton;

        public event Action onMoveUIEvent;
        public event Action onBuildPointSequenceEvent;

        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }

        private void LateUpdate()
        {
            onMoveUIEvent?.Invoke();
        }

        private void StartGame()
        {
            onBuildPointSequenceEvent?.Invoke();
            startPanel.transform.DOMoveY(Screen.height, 0.5f).SetEase(Ease.InBack)
                .OnComplete(() => startPanel.SetActive(false));
        }

        private void Pause()
        {
            IsPause = !IsPause;
            Time.timeScale = IsPause ? 0 : 1;
            _menuPanel.SetActive(IsPause);
        }
    }
}