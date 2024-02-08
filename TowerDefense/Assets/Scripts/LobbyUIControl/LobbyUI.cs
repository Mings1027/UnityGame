using System;
using AdsControl;
using CurrencyControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace LobbyUIControl
{
    public class LobbyUI : MonoBehaviour
    {
        public Sequence diamondNotifySequence { get; private set; }
        public Sequence emeraldNotifySequence { get; private set; }

        [field: SerializeField] public DiamondCurrency diamondCurrency { get; private set; }
        [field: SerializeField] public EmeraldCurrency emeraldCurrency { get; private set; }

        [SerializeField] private GameObject buttonsObj;
        [SerializeField] private GameObject inGameMoneyObj;

        [SerializeField] private RectTransform diaNotifyRect;
        [SerializeField] private RectTransform emeraldNotifyRect;
        [SerializeField] private NoticePanel logOutPanel;

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            diamondNotifySequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(diaNotifyRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(diaNotifyRect.DOAnchorPosY(200, 0.25f).SetDelay(2));
            emeraldNotifySequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(emeraldNotifyRect.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(emeraldNotifyRect.DOAnchorPosY(200, 0.25f).SetDelay(2));
        }

        private void Start()
        {
            Init();
        }

        private void OnDisable()
        {
            diamondNotifySequence?.Kill();
        }

        private void Init()
        {
            FindAnyObjectByType<AdmobManager>().BindLobbyUI(this);
            inGameMoneyObj.SetActive(false);
            logOutPanel.OnOkButtonEvent += () => { SceneManager.LoadSceneAsync("LoginScene"); };
        }

        public void SetActiveButtons(bool active, bool inGameMoneyActive)
        {
            buttonsObj.SetActive(active);
            inGameMoneyObj.SetActive(inGameMoneyActive);
        }

        public void On()
        {
            diamondCurrency.On();
            emeraldCurrency.On();
        }

        public void Off()
        {
            diamondCurrency.Off();
            emeraldCurrency.Off();
        }
    }
}