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
    public class TowerCardController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private bool _isDrag;
        private RectTransform _rectTransform;
        private Sequence _slideSequence;
        private InputManager _inputManager;
        private CanvasGroup _canvasGroup;

        private Dictionary<TowerType, TowerData> _towerButtonDic;

        private float _rectHeight;

        [SerializeField] private Button closeButton;
        [SerializeField] private TowerDescriptionCard towerDescriptionCard;

#region Unity Event

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;
            CameraManager.isControlActive = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CameraManager.isControlActive = true;
            _isDrag = false;
            if (_rectTransform.anchoredPosition.y > -_rectHeight * 0.5f)
            {
                _inputManager.TryPlaceTower();
            }
        }

#endregion

        public void Init()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;
            _slideSequence = DOTween.Sequence().SetAutoKill(false).Pause().SetUpdate(true)
                .Append(_canvasGroup.DOFade(1, 0.25f).From(0))
                .Join(_rectTransform.DOAnchorPosY(0, 0.25f).From(new Vector2(0, -100)));
            _slideSequence.OnComplete(() =>
            {
                UIManager.ShowTowerButton();
                _canvasGroup.blocksRaycasts = true;
            });
            _slideSequence.OnRewind(() => _canvasGroup.blocksRaycasts = false);

            _inputManager = FindAnyObjectByType<InputManager>();
            _towerButtonDic = new Dictionary<TowerType, TowerData>();

            _rectHeight = _rectTransform.rect.height;

            var towerButtons = transform.GetChild(0);
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButton = towerButtons.GetChild(i).GetComponent<TowerButton>();
                towerButton.OnOpenCardEvent += OpenCard;
                towerButton.OnCamDisableEvent += CameraManager.SetCameraActive;
                towerButton.OnCloseCardEvent += CloseTowerCard;
                towerButton.OnCamEnableEvent += () => CameraManager.isControlActive = true;
                towerButton.OnBeginDragEvent += OnBeginDrag;
                towerButton.OnStartPlacement += StartPlacement;
                towerButton.OnEndDragEvent += OnEndDrag;
            }

            closeButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
               
                _slideSequence.PlayBackwards();
                CloseTowerCard();
                UIManager.SlideDown().Forget();
            });
        }

        private void OpenCard(TowerType towerType, RectTransform buttonRect)
        {
            if (_isDrag) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            towerDescriptionCard.OpenTowerCard(_towerButtonDic[towerType].towerType, buttonRect);
        }

        private void StartPlacement(TowerType towerType)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[towerType].towerType, _towerButtonDic[towerType].isUnitTower);
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

        public void SlideUp()
        {
            _slideSequence.Restart();
        }
    }
}