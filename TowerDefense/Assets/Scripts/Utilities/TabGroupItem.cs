using System;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class TabGroupItem : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Tween _groupTween;
        private TMP_Text _groupButtonText;

        public event Action<TabGroupItem> OnTabEvent;

        [SerializeField] private Button groupButton;
        [SerializeField] private Image itemImage;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color unSelectedColor;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;

            _groupButtonText = groupButton.GetComponentInChildren<TMP_Text>();
            groupButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                OnTabEvent?.Invoke(this);
            });

            _groupTween = _canvasGroup.DOFade(1, 0.1f).From(0).SetAutoKill(false).Pause();
            _groupTween.OnComplete(() => _canvasGroup.blocksRaycasts = true);
        }

        public void OpenGroup()
        {
            itemImage.transform.DOScale(1.1f, 0.25f).From(1);
            itemImage.color = selectedColor;
            _groupButtonText.DOScale(1.1f, 0.25f).From(1);
            _groupButtonText.color = selectedColor;
            _groupTween.Restart();
        }

        public void CloseGroup()
        {
            itemImage.transform.DOScale(1, 0.25f).From(1.1f);
            itemImage.color = unSelectedColor;
            _groupButtonText.DOScale(1, 0.25f).From(1.1f);
            _groupButtonText.color = unSelectedColor;
            _canvasGroup.blocksRaycasts = false;
            _groupTween.PlayBackwards();
        }
    }
}