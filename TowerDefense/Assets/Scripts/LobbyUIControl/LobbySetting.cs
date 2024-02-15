using System.Collections.Generic;
using System.Globalization;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

namespace LobbyUIControl
{
    public class LobbySetting : MonoBehaviour
    {
        private bool _isOpen;
        private Tween _settingPanelSequence;
        private Queue<TabGroupItem> _tabGroupItemQueue;
        private TabGroupItem[] _tabGroupArray;
        private CanvasGroup _settingPanelGroup;

        [SerializeField] private Button settingButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private RectTransform tabGroups;

        private void Start()
        {
            _tabGroupItemQueue = new Queue<TabGroupItem>();
            _settingPanelGroup = GetComponent<CanvasGroup>();
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
        }

        private void OpenSetting()
        {
            _settingPanelSequence.OnComplete(() => _settingPanelGroup.blocksRaycasts = true).Restart();
        }

        private void CloseSetting()
        {
            _settingPanelSequence.OnRewind(() => _settingPanelGroup.blocksRaycasts = false).PlayBackwards();
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