using BackEnd;
using BackendControl;
using CurrencyControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LobbyControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class ExchangeController : MonoBehaviour
    {
        private int _curQuantity;
        private const byte EmeraldAmount = 100;
        private LobbyUI _lobbyUI;
        private Tween _panelTween;
        private Sequence _loadingSequence;
        [SerializeField] private RectTransform shopRectTransform;
        [SerializeField] private Button emeraldButton;
        [SerializeField] private Image blockImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minButton;
        [SerializeField] private Button maxButton;
        [SerializeField] private TMP_Text diamondCostText;
        [SerializeField] private TMP_Text emeraldCostText;
        [SerializeField] private Button exchangeButton;
        [SerializeField] private Image loadingBlockImage;
        [SerializeField] private Image loadingImage;

        private void Awake()
        {
            _lobbyUI = FindAnyObjectByType<LobbyUI>();
            _panelTween = shopRectTransform.DOScaleX(1, 0.25f).From(0).SetEase(Ease.OutBack).SetAutoKill(false).Pause();
            _loadingSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(loadingImage.transform.DOScale(1, 0.25f).From(0).SetEase(Ease.OutBack).SetUpdate(true))
                .Join(loadingImage.transform.DOLocalRotate(new Vector3(0, 0, -360), 1, RotateMode.FastBeyond360)
                    .SetLoops(30));
            blockImage.enabled = false;
            loadingBlockImage.enabled = false;
            loadingImage.enabled = false;
            emeraldButton.onClick.AddListener(OpenPanel);
            closeButton.onClick.AddListener(ClosePanel);
            minusButton.onClick.AddListener(MinusQuantity);
            plusButton.onClick.AddListener(PlusQuantity);
            minButton.onClick.AddListener(MinQuantity);
            maxButton.onClick.AddListener(MaxQuantity);
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
            if (BackendGameData.curTbc >= _curQuantity)
            {
                BackendGameData.curTbc -= _curQuantity * EmeraldAmount;
                BackendGameData.userData.emerald += _curQuantity * EmeraldAmount;

                TbcAsync(_curQuantity);
                _curQuantity = 0;
                quantityText.text = "0";
                diamondCostText.text = "0";
                emeraldCostText.text = "0";
            }
            else
            {
                _lobbyUI.diamondNotifySequence.Restart();
            }
        }

        private void TbcAsync(int count)
        {
            loadingBlockImage.enabled = true;
            loadingImage.enabled = true;
            _loadingSequence.Restart();

            UniTask.RunOnThreadPool(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    Backend.TBC.UseTBC("18c396e0-bf34-11ee-86ea-2f0a13d84ab2", "에메랄드 교환");
                }
            });

            _lobbyUI.diamondCurrency.SetText();
            _lobbyUI.emeraldCurrency.SetText();
            _loadingSequence.Pause();
            loadingBlockImage.enabled = false;
            loadingImage.enabled = false;
        }

        private void OpenPanel()
        {
            _lobbyUI.SetActiveButtons(false, true);
            _lobbyUI.Off();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = true;
            _panelTween.Restart();

            _curQuantity = 0;
            quantityText.text = _curQuantity.ToString();
            diamondCostText.text = "0";
            emeraldCostText.text = "0";
        }

        private void ClosePanel()
        {
            _lobbyUI.SetActiveButtons(true, false);
            _lobbyUI.On();
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            blockImage.enabled = false;
            _panelTween.PlayBackwards();
        }

        private void PlusQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (BackendGameData.curTbc <= _curQuantity * EmeraldAmount) return;
            _curQuantity++;
            quantityText.text = _curQuantity.ToString();
            diamondCostText.text = $"- {_curQuantity * EmeraldAmount}";
            emeraldCostText.text = $"+ {_curQuantity * EmeraldAmount}";
        }

        private void MinusQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_curQuantity <= 0) return;
            _curQuantity--;
            quantityText.text = _curQuantity.ToString();
            diamondCostText.text = $"- {_curQuantity * EmeraldAmount}";
            emeraldCostText.text = $"+ {_curQuantity * EmeraldAmount}";
        }

        private void MinQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity = 0;
            quantityText.text = "0";
            diamondCostText.text = "0";
            emeraldCostText.text = "0";
        }

        private void MaxQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity = BackendGameData.curTbc / EmeraldAmount;
            quantityText.text = _curQuantity.ToString();
            diamondCostText.text = $"- {_curQuantity * EmeraldAmount}";
            emeraldCostText.text = $"+ {_curQuantity * EmeraldAmount}";
        }
    }
}