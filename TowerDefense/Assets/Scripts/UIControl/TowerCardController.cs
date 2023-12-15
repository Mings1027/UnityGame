using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataControl;
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
        private Tweener _slideDownTween;
        private Tweener _slideUpTween;
        private InputManager _inputManager;
        private CameraManager _cameraManager;

        private Dictionary<int, TowerData> _towerButtonDic;

        private float _rectHeight;

        [SerializeField] private TowerCardUI towerCardUI;

        #region Unity Event

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;
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
            _isStartPlacement = false;
            _isDrag = false;
            if (_rectTransform.anchoredPosition.y > -_rectHeight * 0.5f)
            {
                _slideUpTween.ChangeStartValue(_rectTransform.anchoredPosition).ChangeEndValue(Vector2.zero).Restart();
                _inputManager.TryPlaceTower();
            }
            else
            {
                _slideDownTween.ChangeStartValue(_rectTransform.anchoredPosition)
                    .OnComplete(() => UIManager.Instance.SlideDown().Forget()).Restart();
            }
        }

        #endregion

        public void Init()
        {
            _rectTransform = GetComponent<RectTransform>();
            _slideDownTween = _rectTransform.DOAnchorPosY(-250, 0.25f).SetAutoKill(false).Pause();
            _slideUpTween = _rectTransform.DOAnchorPosY(-250, 0.25f).From().SetAutoKill(false).Pause();

            if (Camera.main != null) _cameraManager = Camera.main.GetComponentInParent<CameraManager>();
            _inputManager = FindObjectOfType<InputManager>();
            _towerButtonDic = new Dictionary<int, TowerData>();

            _rectHeight = _rectTransform.rect.height;

            var towerButtons = transform.GetChild(0);
            for (var i = 0; i < towerButtons.childCount; i++)
            {
                var towerButton = towerButtons.GetChild(i).GetComponent<TowerButton>();
                towerButton.buttonIndex = (byte)i;
                towerButton.OnPointerDownEvent += PointerDown;
                towerButton.OnCamDisableEvent += () => _cameraManager.DisableForPlaceTower();
                towerButton.OnPointerUpEvent += CloseTowerCard;
                towerButton.OnCamEnableEvent += () => _cameraManager.enabled = true;
                towerButton.OnBeginDragEvent += OnBeginDrag;
                towerButton.OnStartPlacement += StartPlacement;
                towerButton.OnDragEvent += OnDrag;
                towerButton.OnEndDragEvent += OnEndDrag;
            }
        }

        private void PointerDown(int index, Transform buttonPos) =>
            OpenCardAsync(_towerButtonDic[index], buttonPos).Forget();

        private async UniTaskVoid OpenCardAsync(TowerData towerData, Transform buttonPos)
        {
            await UniTask.Delay(300);
            if (_isDrag) return;
            if (TowerButton.IsOnButton)
            {
                towerCardUI.OpenTowerCard(towerData, buttonPos);
            }
        }

        private void StartPlacement(int index)
        {
            _inputManager.enabled = true;
            _inputManager.StartPlacement(_towerButtonDic[index].TowerType, _towerButtonDic[index].IsUnitTower);
        }

        private void CloseTowerCard()
        {
            if (towerCardUI.IsOpen)
            {
                towerCardUI.CloseCard();
            }
        }

        public void SetDictionary(int index, TowerData towerData) => _towerButtonDic.Add(index, towerData);

        public void SlideUp() => _slideUpTween.ChangeStartValue(_rectTransform.anchoredPosition).Restart();
    }
}