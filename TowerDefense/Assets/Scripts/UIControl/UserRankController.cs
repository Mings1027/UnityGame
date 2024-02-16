using System;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using LobbyUIControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class UserRankController : MonoBehaviour
    {
        private LobbyUI _lobbyUI;
        private Sequence _panelSequence;

        [SerializeField] private Image blockImage;
        [SerializeField] private CanvasGroup rankPanelGroup;
        [SerializeField] private Button rankButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject userRankItem;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            _panelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(rankPanelGroup.DOFade(1, 0.25f).From(0))
                .Join(rankPanelGroup.GetComponent<RectTransform>().DOAnchorPosX(0, 0.25f).From(new Vector2(100, 0)));
            blockImage.enabled = false;
            rankPanelGroup.blocksRaycasts = false;
            rankButton.onClick.AddListener(OpenRankPanel);
            closeButton.onClick.AddListener(CloseRankPanel);
        }

        private void Start()
        {
            SetRanking();
        }

        private void OnDisable()
        {
            _panelSequence?.Kill();
        }

        private void OpenRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _panelSequence.OnComplete(() => rankPanelGroup.blocksRaycasts = true).Restart();
            _lobbyUI.SetActiveButtons(false, false);
        }

        private void CloseRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            _panelSequence.OnRewind(() => rankPanelGroup.blocksRaycasts = false).PlayBackwards();
            _lobbyUI.SetActiveButtons(true, false);
        }

        public void SetRanking()
        {
            BackendRank.instance.RankGet();
            if (content.childCount > 0)
            {
                for (int i = 0; i < content.childCount; i++)
                {
                    Destroy(content.GetChild(i).gameObject);
                }
            }

            var userRankInfos = BackendRank.userRankInfos;
            for (var i = 0; i < userRankInfos.Length; i++)
            {
                var item = Instantiate(userRankItem, content).transform;
                item.GetChild(0).GetComponent<TMP_Text>().text = userRankInfos[i].rank;
                item.GetChild(1).GetComponent<TMP_Text>().text = userRankInfos[i].nickName;
                item.GetChild(2).GetComponent<TMP_Text>().text = userRankInfos[i].score;
            }
        }
    }
}