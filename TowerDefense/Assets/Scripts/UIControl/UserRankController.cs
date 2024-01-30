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
        private UserRankInfo[] _userRankInfos;

        [SerializeField] private GameObject buttons;
        [SerializeField] private Image blockImage;
        [SerializeField] private RectTransform rankPanel;
        [SerializeField] private Button rankButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject userRankItem;

        private void Awake()
        {
            blockImage.enabled = false;
            rankButton.onClick.AddListener(OpenRankPanel);
            closeButton.onClick.AddListener(CloseRankPanel);
            rankPanel.localScale = Vector3.zero;
        }

        private void Start()
        {
            SetRanking();
        }

        private void OpenRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            rankPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
            buttons.SetActive(false);
        }

        private void CloseRankPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            rankPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
            buttons.SetActive(true);
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