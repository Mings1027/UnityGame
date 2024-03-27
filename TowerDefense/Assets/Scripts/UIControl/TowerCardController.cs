using System.Collections.Generic;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerCardController : MonoBehaviour
    {
        private bool _isDrag;
        private bool _isShowTowerBtn;
        private RectTransform _towerCardGroupRect;
        private Sequence _towerCardSequence;
        private InputManager _inputManager;
        private Dictionary<TowerType, TowerData> _towerButtonDic;

        public Tween scaleTween { get; private set; }
        public bool isPointerOver { get; private set; }

        [SerializeField] private CanvasGroup openTowerCardBtnGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private TowerDescriptionCard towerDescriptionCard;
        [SerializeField] private CanvasGroup towerCardGroup;

        public void Init()
        {
            _towerCardGroupRect = towerCardGroup.GetComponent<RectTransform>();
            towerCardGroup.blocksRaycasts = false;
            _towerCardSequence = DOTween.Sequence().SetAutoKill(false).Pause().SetUpdate(true)
                .Append(towerCardGroup.DOFade(1, 0.25f).From(0))
                .Join(_towerCardGroupRect.DOAnchorPosX(0, 0.25f).From(new Vector2(-100, 0)));
            _towerCardSequence.OnComplete(() => towerCardGroup.blocksRaycasts = true);

            scaleTween = transform.DOScale(0.8f, 0.25f).From(1).SetAutoKill(false).Pause().SetUpdate(true);
            _inputManager = FindAnyObjectByType<InputManager>();
            _towerButtonDic = new Dictionary<TowerType, TowerData>();

            for (var i = 0; i < towerCardGroup.transform.childCount; i++)
            {
                if (towerCardGroup.transform.GetChild(i).TryGetComponent(out TowerButton towerButton))
                {
                    towerButton.OnOpenCardEvent += OpenCard;
                    towerButton.OnStartPlacement += StartPlacement;
                    towerButton.OnPointerOverEvent += PointerOver;
                    towerButton.OnCamDisableEvent += CameraManager.SetCameraActive;
                    towerButton.OnCamEnableEvent += () => CameraManager.isControlActive = true;
                    towerButton.OnCloseCardEvent += CloseTowerCard;
                    towerButton.OnTryPlaceTowerEvent += TryPlaceTower;
                }
            }

            openTowerCardBtnGroup.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isShowTowerBtn) return;
                _isShowTowerBtn = true;
                openTowerCardBtnGroup.DOFade(0, 0);
                openTowerCardBtnGroup.blocksRaycasts = false;
                _towerCardSequence.Restart();
            });
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);

                openTowerCardBtnGroup.DOFade(1, 0);
                openTowerCardBtnGroup.blocksRaycasts = true;

                towerCardGroup.blocksRaycasts = false;
                _towerCardSequence.PlayBackwards();
                CloseTowerCard();
                DisappearToggleBtn().Forget();
            });
        }

        private void OpenCard(TowerType towerType)
        {
            if (_isDrag) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            towerDescriptionCard.OpenTowerCard(_towerButtonDic[towerType]);
        }

        private void StartPlacement(TowerType towerType)
        {
            _isDrag = true;
            CameraManager.isControlActive = false;
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[towerType].towerType, _towerButtonDic[towerType].isUnitTower);
        }

        private void PointerOver(bool value)
        {
            isPointerOver = value;
        }

        private void TryPlaceTower()
        {
            if (isPointerOver) return;
            CameraManager.isControlActive = true;
            _isDrag = false;
            _inputManager.TryPlaceTower();
        }

        public void AppearToggleButton()
        {
            if (!_isShowTowerBtn)
            {
                openTowerCardBtnGroup.DOFade(1, 0.2f).OnComplete(() => openTowerCardBtnGroup.blocksRaycasts = true);
            }
        }

        private async UniTaskVoid DisappearToggleBtn()
        {
            _isShowTowerBtn = false;
            await UniTask.Delay(2000, cancellationToken: this.GetCancellationTokenOnDestroy());
            if (!_isShowTowerBtn)
            {
                openTowerCardBtnGroup.DOFade(0, 1).OnComplete(() => openTowerCardBtnGroup.blocksRaycasts = false);
            }
        }

        private void CloseTowerCard()
        {
            if (towerDescriptionCard.isOpen)
            {
                towerDescriptionCard.CloseCard();
            }
        }

        public void SetDictionary(TowerType towerType, TowerData towerData)
        {
            _towerButtonDic.Add(towerType, towerData);
        }
    }
}