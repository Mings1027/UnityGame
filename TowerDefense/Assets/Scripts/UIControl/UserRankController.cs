using System;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UserRankController : MonoBehaviour
    {
        private bool _isOpenRankPanel;
        private RectTransform _rankPanel;
        private UserRankInfo[] _userRankInfos;

        [SerializeField] private Button rankButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject userRankItem;

        private void Awake()
        {
            _rankPanel = GetComponent<RectTransform>();
            rankButton.onClick.AddListener(OpenRankPanel);
            closeButton.onClick.AddListener(CloseRankPanel);
            _rankPanel.localScale = Vector3.zero;
        }

        private void Start()
        {
            SetRanking();
        }

        private void OpenRankPanel()
        {
            _isOpenRankPanel = true;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _rankPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
        }

        private void CloseRankPanel()
        {
            _isOpenRankPanel = false;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _rankPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
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