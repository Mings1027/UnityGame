using BackEnd;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyUIControl
{
    public class ExchangeController : MonoBehaviour
    {
        private int _curQuantity;
        private const byte EmeraldAmount = 100;
        private LobbyUI _lobbyUI;
        private Sequence _shopPanelSequence;

        [SerializeField] private CanvasGroup shopPanelGroup;
        [SerializeField] private Button emeraldButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minButton;
        [SerializeField] private Button maxButton;
        [SerializeField] private TMP_Text diamondCostText;
        [SerializeField] private TMP_Text emeraldCostText;

        [SerializeField] private Button exchangeButton;

        private void Awake()
        {
            _lobbyUI = GetComponentInParent<LobbyUI>();
            shopPanelGroup.blocksRaycasts = false;
            var shopPanelRect = shopPanelGroup.GetComponent<RectTransform>();
            _shopPanelSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(shopPanelGroup.DOFade(1, 0.25f).From(0))
                .Join(shopPanelRect.DOAnchorPosX(0, 0.25f).From(new Vector2(100, 0)));
            _shopPanelSequence.OnComplete(() => shopPanelGroup.blocksRaycasts = true);
            _shopPanelSequence.OnRewind(() => { _lobbyUI.OffBlockImage(); });
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
            _shopPanelSequence?.Kill();
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
                _lobbyUI.NoticeTween(NoticeTableEnum.NeedMoreDia);
            }
        }

        private void TbcAsync(int count)
        {
            UniTask.RunOnThreadPool(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    Backend.TBC.UseTBC("18c396e0-bf34-11ee-86ea-2f0a13d84ab2", "에메랄드 교환");
                }
            });

            _lobbyUI.diamondCurrency.SetText();
            _lobbyUI.emeraldCurrency.SetText();
        }

        private void OpenPanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.SetActiveButtons(false, true);
            _lobbyUI.Off();
            _lobbyUI.OnBackgroundImage();
            _shopPanelSequence.Restart();

            _curQuantity = 0;
            quantityText.text = _curQuantity.ToString();
            diamondCostText.text = "0";
            emeraldCostText.text = "0";
        }

        private void ClosePanel()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _lobbyUI.SetActiveButtons(true, false);
            _lobbyUI.On();
            _lobbyUI.OffBackgroundImage();
            shopPanelGroup.blocksRaycasts = false;
            _shopPanelSequence.PlayBackwards();
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

        private void MaxQuantity()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _curQuantity = BackendGameData.curTbc / EmeraldAmount;
            if (_curQuantity <= 0) return;
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
    }
}