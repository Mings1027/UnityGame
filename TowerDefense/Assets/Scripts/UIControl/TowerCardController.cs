using System.Collections.Generic;
using CustomEnumControl;
using DataControl.TowerDataControl;
using DG.Tweening;
using ManagerControl;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIControl
{
    public class TowerCardController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private bool _isStartPlacement;
        private bool _isDrag;
        private RectTransform _rectTransform;
        private Tweener _slideTween;
        private InputManager _inputManager;

        private Dictionary<int, TowerData> _towerButtonDic;

        private float _rectHeight;

        [SerializeField] private TowerDescriptionCard towerDescriptionCard;

        #region Unity Event

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;
            UIManager.Instance.CameraManager.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.position.y > _rectHeight + 50)
            {
                _isStartPlacement = true;
            }

            var rectAnchorPos = _rectTransform.anchoredPosition;
            if (_isStartPlacement)
            {
                if (eventData.delta.y > 0)
                {
                    rectAnchorPos += new Vector2(0, eventData.delta.y);
                }
            }
            else
            {
                rectAnchorPos += new Vector2(0, eventData.delta.y);
            }

            if (rectAnchorPos.y > 0) rectAnchorPos = Vector2.zero;
            else if (rectAnchorPos.y < -_rectHeight) rectAnchorPos = new Vector2(0, -_rectHeight);
            _rectTransform.anchoredPosition = rectAnchorPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UIManager.Instance.CameraManager.enabled = true;
            _isStartPlacement = false;
            _isDrag = false;
            if (_rectTransform.anchoredPosition.y > -_rectHeight * 0.5f)
            {
                //up
                _slideTween.ChangeStartValue(_rectTransform.anchoredPosition).Restart();
                _inputManager.TryPlaceTower();
            }
            else
            {
                //down
                _slideTween.ChangeStartValue(_rectTransform.anchoredPosition)
                    .ChangeEndValue(new Vector2(0, -250))
                    .OnComplete(() => { UIManager.Instance.SlideDown().Forget(); }).Restart();
            }
        }

        #endregion

        public void Init()
        {
            _rectTransform = GetComponent<RectTransform>();
            _slideTween = _rectTransform.DOAnchorPosY(-250, 0.25f).From().SetAutoKill(false).Pause();

            _inputManager = (InputManager)FindAnyObjectByType(typeof(InputManager));
            _towerButtonDic = new Dictionary<int, TowerData>();

            _rectHeight = _rectTransform.rect.height;

            var towerButtons = transform.GetChild(0);
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButton = towerButtons.GetChild(i).GetComponent<TowerButton>();
                towerButton.buttonIndex = (byte)i;
                towerButton.OnOpenCardEvent += OpenCard;
                towerButton.OnCamDisableEvent += () => UIManager.Instance.CameraManager.DisableForPlaceTower();
                towerButton.OnCloseCardEvent += CloseTowerCard;
                towerButton.OnCamEnableEvent += () => UIManager.Instance.CameraManager.enabled = true;
                towerButton.OnBeginDragEvent += OnBeginDrag;
                towerButton.OnStartPlacement += StartPlacement;
                towerButton.OnDragEvent += OnDrag;
                towerButton.OnEndDragEvent += OnEndDrag;
            }
        }

        private void OpenCard(int index, Transform buttonPos)
        {
            if (_isDrag) return;
            SoundManager.Instance.PlayUISound(SoundEnum.ButtonSound);
            towerDescriptionCard.OpenTowerCard(_towerButtonDic[index].TowerType, buttonPos);
        }

        private void StartPlacement(int index)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[index].TowerType, _towerButtonDic[index].IsUnitTower);
        }

        private void CloseTowerCard()
        {
            if (towerDescriptionCard.IsOpen)
            {
                towerDescriptionCard.CloseCard();
            }
        }

        public void SetDictionary(int index, TowerData towerData) => _towerButtonDic.Add(index, towerData);

        public void SlideUp()
        {
            _slideTween.ChangeStartValue(_rectTransform.anchoredPosition)
                .ChangeEndValue(Vector2.zero).OnComplete(() => UIManager.Instance.ShowTowerButton()).Restart();
        }
    }
}