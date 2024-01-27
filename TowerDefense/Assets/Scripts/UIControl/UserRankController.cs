using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class UserRankController : MonoBehaviour
    {
        private bool _isOpenRankPanel;
        private Button _rankButton;
        [SerializeField] private RectTransform rankPanel;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            _rankButton = GetComponent<Button>();
            _rankButton.onClick.AddListener(OpenRankPanel);
            closeButton.onClick.AddListener(CloseRankPanel);
            rankPanel.localScale = Vector3.zero;
        }

        private void OpenRankPanel()
        {
            _isOpenRankPanel = true;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            rankPanel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack);
        }

        private void CloseRankPanel()
        {
            _isOpenRankPanel = false;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            rankPanel.DOScale(0, 0.25f).From(1).SetEase(Ease.InBack);
        }
    }
}