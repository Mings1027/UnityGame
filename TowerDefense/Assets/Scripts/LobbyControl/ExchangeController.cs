using BackEnd;
using BackendControl;
using CustomEnumControl;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class ExchangeController : MonoBehaviour
    {
        private int _emeraldAmount;

        private Tween _panelTween;
        private Sequence _needMoreSequence;
        private CurrencyController _currencyController;
        [SerializeField] private GameObject buttons;
        [SerializeField] private Transform inGameMoney;
        [SerializeField] private RectTransform shopRectTransform;
        [SerializeField] private Button emeraldButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text emeraldAmountText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button exchangeButton;
        [SerializeField] private RectTransform needMoreDiaTransform;

        private void Awake()
        {
            _currencyController = FindAnyObjectByType<CurrencyController>();
            _panelTween = shopRectTransform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _needMoreSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(needMoreDiaTransform.DOAnchorPosX(-100, 0.25f).From(new Vector2(600, -50)))
                .Append(needMoreDiaTransform.DOAnchorPosY(200, 0.25f).SetDelay(1));
            blockImage.enabled = false;
            emeraldButton.onClick.AddListener(OpenPanel);
            closeButton.onClick.AddListener(ClosePanel);
            minusButton.onClick.AddListener(MinusEmerald);
            plusButton.onClick.AddListener(PlusEmerald);
            exchangeButton.onClick.AddListener(ExchangeEmerald);
        }

        private void ExchangeEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_emeraldAmount <= 0) return;
            if (BackendGameData.curTbc > 50)
            {
                var useTbc = Backend.TBC.UseTBC("18c396e0-bf34-11ee-86ea-2f0a13d84ab2", "에메랄드 교환");
                if (useTbc.IsSuccess())
                {
                    BackendGameData.userData.emerald += _emeraldAmount;
                    _emeraldAmount = 0;
                    emeraldAmountText.text = _emeraldAmount.ToString();
                    _currencyController.diamondCurrency.SetText();
                    _currencyController.emeraldCurrency.SetText();
                }
            }
            else
            {
                _needMoreSequence.Restart();
            }
        }

        private void OnDisable()
        {
            _panelTween?.Kill();
        }

        private void OpenPanel()
        {
            buttons.SetActive(false);
            inGameMoney.SetParent(transform);
            _currencyController.Off();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _panelTween.Restart();

            _emeraldAmount = 0;
            emeraldAmountText.text = _emeraldAmount.ToString();
        }

        private void ClosePanel()
        {
            buttons.SetActive(true);
            inGameMoney.SetParent(buttons.transform);
            _currencyController.On();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            _panelTween.PlayBackwards();
        }

        private void PlusEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _emeraldAmount += 50;
            emeraldAmountText.text = _emeraldAmount.ToString();
        }

        private void MinusEmerald()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_emeraldAmount <= 0) return;
            _emeraldAmount -= 50;
            emeraldAmountText.text = _emeraldAmount.ToString();
        }
    }
}