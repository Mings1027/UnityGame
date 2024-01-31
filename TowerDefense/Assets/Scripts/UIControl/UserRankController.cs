using System;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using LobbyUIControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UserRankController : MonoBehaviour
    {
        private UserRankInfo[] _userRankInfos;
        private LobbyUI _lobbyUI;
        private Tween _panelTween;

        [SerializeField] private Image blockImage;
        [SerializeField] private RectTransform rankPanel;
        [SerializeField] private Button rankButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject userRankItem;

        private void Awake()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _panelTween = rankPanel.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            blockImage.enabled = false;
            rankButton.onClick.AddListener(OpenRankPanel);
            closeButton.onClick.AddListener(CloseRankPanel);
        }

        private void Start()
        {
            SetRanking();
        }

        private void OnDisable()
        {
            _panelTween?.Kill();
        }

        private void OpenRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _panelTween.Restart();
            _lobbyUI.SetActiveButtons(false, false);
        }

        private void CloseRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            _panelTween.PlayBackwards();
            _lobbyUI.SetActiveButtons(true, false);
        }

        private void SetRanking()
        {
            _userRankInfos = BackendRank.instance.RankGet();
            for (var i = 0; i < _userRankInfos.Length; i++)
            {
                var item = Instantiate(userRankItem, content).transform;
                item.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = _userRankInfos[i].rank;
                item.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = _userRankInfos[i].nickName;
                item.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = _userRankInfos[i].score;
            }
        }
    }
}