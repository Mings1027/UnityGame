using System.Collections.Generic;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace LobbyUIControl
{
    public class ShopManager : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Sequence _shopPanelSequence;
        private TabGroupItem[] _tabGroupItems;
        private Queue<TabGroupItem> _tabGroupItemQueue;
        private CanvasGroup _shopPanelGroup;

        [SerializeField] private Button shopButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private RectTransform tabGroups;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _shopPanelGroup = GetComponent<CanvasGroup>();
            var shopRect = GetComponent<RectTransform>();
            
            _shopPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(_shopPanelGroup.DOFade(1, 0.25f).From(0))
                .Join(shopRect.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            _shopPanelSequence.OnComplete(() => _shopPanelGroup.blocksRaycasts = true);
            _shopPanelSequence.OnRewind(() => _lobbyUI.OffBlockImage());
            _shopPanelGroup.blocksRaycasts = false;
            Init();
            ButtonInit();
        }

        private void Init()
        {
            _tabGroupItems = new TabGroupItem[tabGroups.childCount];
            for (int i = 0; i < _tabGroupItems.Length; i++)
            {
                _tabGroupItems[i] = tabGroups.GetChild(i).GetComponent<TabGroupItem>();
                _tabGroupItems[i].OnTabEvent += ClickTab;
            }

            _tabGroupItemQueue = new Queue<TabGroupItem>();
        }

        private void ClickTab(TabGroupItem tabGroupItem)
        {
            var tabItem = _tabGroupItemQueue.Dequeue();
            if (tabItem != tabGroupItem) tabItem.CloseGroup();
            _tabGroupItemQueue.Enqueue(tabGroupItem);
            if (tabItem != tabGroupItem) tabGroupItem.OpenGroup();
        }

        private void ButtonInit()
        {
            shopButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _lobbyUI.OnBackgroundImage();
                _lobbyUI.SetActiveButtons(false, true);
                _lobbyUI.Off();
                _shopPanelSequence.Restart();
                _tabGroupItems[0].OpenGroup();
                _tabGroupItemQueue.Enqueue(_tabGroupItems[0]);
            });

            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                _shopPanelGroup.blocksRaycasts = false;
                _lobbyUI.OffBackgroundImage();
                _lobbyUI.SetActiveButtons(true, false);
                _lobbyUI.On();
                _shopPanelSequence.PlayBackwards();

                var count = _tabGroupItemQueue.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _tabGroupItemQueue.Dequeue().CloseGroup();
                    }
                }
            });
        }
    }
}