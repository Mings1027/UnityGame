using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class NotificationPanel : MonoBehaviour
    {
        private Tween _panelTween;

        [SerializeField] private Image blockImage;
        [SerializeField] private RectTransform panel;

        [SerializeField] private Button popUpButton;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;

        public event Action OnPopUpButtonEvent;
        public event Action OnOkButtonEvent;
        public event Action OnCancelButtonEvent;

        private void Awake()
        {
            _panelTween = panel.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            blockImage.enabled = false;

            popUpButton.onClick.AddListener(() =>
            {
                blockImage.enabled = true;
                _panelTween.Restart();
                OnPopUpButtonEvent?.Invoke();
            });
            okButton.onClick.AddListener(() =>
            {
                blockImage.enabled = false;
                _panelTween.PlayBackwards();
                OnOkButtonEvent?.Invoke();
            });
            cancelButton.onClick.AddListener(() =>
            {
                blockImage.enabled = false;
                _panelTween.PlayBackwards();
                OnCancelButtonEvent?.Invoke();
            });
        }
    }
}