using System.Collections.Generic;
using System.Linq;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UIControl;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utilities;

namespace LobbyUIControl
{
    public class LobbySetting : MonoBehaviour
    {
        private FadeController _fadeController;
        private LobbyUI _lobbyUI;
        private bool _isOpen;
        private Tween _settingPanelSequence;
        private Queue<TabGroupItem> _tabGroupItemQueue;
        private TabGroupItem[] _tabGroupArray;
        private CanvasGroup _settingPanelGroup;

        [SerializeField] private Button settingButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private RectTransform tabGroups;
        [SerializeField] private NoticePanel logOutPanel;

        private void Start()
        {
            _fadeController = FindAnyObjectByType<FadeController>();
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _tabGroupItemQueue = new Queue<TabGroupItem>();
            _settingPanelGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
            _settingPanelGroup.blocksRaycasts = false;
            Init();
            InitButton();
            InitTween();
        }

        private void OnDestroy()
        {
            _settingPanelSequence?.Kill();
        }

        private void Init()
        {
            _tabGroupArray = new TabGroupItem[tabGroups.childCount];
            for (int i = 0; i < _tabGroupArray.Length; i++)
            {
                _tabGroupArray[i] = tabGroups.GetChild(i).GetComponent<TabGroupItem>();
                _tabGroupArray[i].OnTabEvent += ClickTab;
            }

            logOutPanel.OnConfirmButtonEvent += () =>
            {
                BackendChart.instance.InitItemTable();
                BackendLogin.instance.LogOut();
                _fadeController.FadeOutScene("LoginScene").Forget();
            };
        }

        private void ClickTab(TabGroupItem tabGroupItem)
        {
            var tabItem = _tabGroupItemQueue.Dequeue();
            if (tabItem != tabGroupItem) tabItem.CloseGroup();
            _tabGroupItemQueue.Enqueue(tabGroupItem);
            if (tabItem != tabGroupItem) tabGroupItem.OpenGroup();
        }

        private void InitButton()
        {
            settingButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OpenSetting();
                _tabGroupArray[0].OpenGroup();
                _tabGroupItemQueue.Enqueue(_tabGroupArray[0]);
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                CloseSetting();
            });
        }

        private void InitTween()
        {
            _settingPanelSequence = _settingPanelGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _settingPanelSequence.OnComplete(() => _settingPanelGroup.blocksRaycasts = true);
            _settingPanelSequence.OnRewind(() =>
            {
                _settingPanelGroup.blocksRaycasts = false;
                _lobbyUI.OffBlockImage();
            });
        }

        private void OpenSetting()
        {
            _lobbyUI.OnBackgroundImage();
            _lobbyUI.SetActiveButtons(false, false);
            _settingPanelSequence.Restart();
        }

        private void CloseSetting()
        {
            _lobbyUI.OffBackgroundImage();
            _lobbyUI.SetActiveButtons(true, false);
            _settingPanelSequence.PlayBackwards();
            if (_tabGroupItemQueue.Count > 0)
            {
                for (int i = 0; i < _tabGroupItemQueue.Count; i++)
                {
                    _tabGroupItemQueue.Dequeue().CloseGroup();
                }
            }
        }
    }
}