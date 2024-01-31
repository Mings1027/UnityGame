using BackEnd;
using BackendControl;
using CurrencyControl;
using CustomEnumControl;
using DG.Tweening;
using LobbyControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class ExchangeController : MonoBehaviour
    {
        private int _curQuantity;
        private const byte EmeraldAmount = 50;
        private LobbyUI _lobbyUI;
        private Tween _panelTween;
        [SerializeField] private RectTransform shopRectTransform;
        [SerializeField] private Button emeraldButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text emeraldAmountText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button exchangeButton;

        private void Awake()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _panelTween = shopRectTransform.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            blockImage.enabled = false;
            emeraldButton.onClick.AddListener(OpenPanel);
            closeButton.onClick.AddListener(ClosePanel);
            minusButton.onClick.AddListener(MinusEmerald);
            plusButton.onClick.AddListener(PlusEmerald);
            exchangeButton.onClick.AddListener(ExchangeEmerald);
        }

        private void OnDisable()
        {
            _panelTween?.Kill();
        }

        private void ExchangeEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_curQuantity <= 0) return;
            if (BackendGameData.curTbc > _curQuantity * EmeraldAmount)
            {
                BackendGameData.curTbc -= _curQuantity * EmeraldAmount;
                var count = _curQuantity;
                for (var i = 0; i < count; i++)
                {
                    Debug.Log($"현재 tbc : {BackendGameData.curTbc}");
                    var index = i;
                    Backend.TBC.UseTBC("18c396e0-bf34-11ee-86ea-2f0a13d84ab2", "에메랄드 교환",
                        _ => { Debug.Log($"use Tbc 완료 {index}"); });
                }

                BackendGameData.userData.emerald += _curQuantity * EmeraldAmount;
                _curQuantity = 0;
                emeraldAmountText.text = _curQuantity.ToString();
                _lobbyUI.diamondCurrency.SetText();
                _lobbyUI.emeraldCurrency.SetText();
            }
            else
            {
                _lobbyUI.diamondNotifySequence.Restart();
            }
        }

        private void OpenPanel()
        {
            _lobbyUI.SetActiveButtons(false, true);
            _lobbyUI.Off();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _panelTween.Restart();

            _curQuantity = 0;
            emeraldAmountText.text = _curQuantity.ToString();
        }

        private void ClosePanel()
        {
            _lobbyUI.SetActiveButtons(true, false);
            _lobbyUI.On();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            _panelTween.PlayBackwards();
        }

        private void PlusEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity++;
            emeraldAmountText.text = (_curQuantity * EmeraldAmount).ToString();
        }

        private void MinusEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_curQuantity <= 0) return;
            _curQuantity--;
            emeraldAmountText.text = (_curQuantity * EmeraldAmount).ToString();
        }
    }
}