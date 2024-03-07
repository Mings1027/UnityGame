using System.Collections.Generic;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class TowerCardController : MonoBehaviour
    {
        private bool _isDrag;
        private RectTransform _towerCardGroupRect;
        private Sequence _towerCardSequence;
        private InputManager _inputManager;
        private Dictionary<TowerType, TowerData> _towerButtonDic;

        public Tween scaleTween { get; private set; }

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
            _towerCardSequence.OnRewind(() => towerCardGroup.blocksRaycasts = false);

            scaleTween = transform.DOScale(0.75f, 0.25f).From(1).SetAutoKill(false).Pause().SetUpdate(true);
            _inputManager = FindAnyObjectByType<InputManager>();
            _towerButtonDic = new Dictionary<TowerType, TowerData>();

            for (var i = 0; i < towerCardGroup.transform.childCount; i++)
            {
                if (towerCardGroup.transform.GetChild(i).TryGetComponent(out TowerButton towerButton))
                {
                    towerButton.OnOpenCardEvent += OpenCard;
                    towerButton.OnCamDisableEvent += CameraManager.SetCameraActive;
                    towerButton.OnCloseCardEvent += CloseTowerCard;
                    towerButton.OnCamEnableEvent += () => CameraManager.isControlActive = true;
                    towerButton.OnStartPlacement += StartPlacement;
                    towerButton.OnTryPlaceTowerEvent += TryPlaceTower;
                }
            }

            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);

                _towerCardSequence.PlayBackwards();
                CloseTowerCard();
                UIManager.DisappearToggleButton();
            });
        }

        private void OpenCard(TowerType towerType)
        {
            if (_isDrag) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            towerDescriptionCard.OpenTowerCard(_towerButtonDic[towerType].towerType);
        }

        private void StartPlacement(TowerType towerType)
        {
            _isDrag = true;
            CameraManager.isControlActive = false;
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[towerType].towerType, _towerButtonDic[towerType].isUnitTower);
        }

        private void TryPlaceTower()
        {
            CameraManager.isControlActive = true;
            _isDrag = false;
            _inputManager.TryPlaceTower();
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

        public void OpenTowerCards()
        {
            towerCardGroup.blocksRaycasts = true;
            _towerCardSequence.Restart();
        }
    }
}