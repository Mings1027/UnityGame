using System;
using DataControl;
using DG.Tweening;
using ManagerControl;
using StatusControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIControl
{
    public class GameHUD : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Tweener _hudSlideTween;
        private Sequence _cantMoveImageSequence;
        private bool _isHUDVisible;
        private RectTransform _rectTransform;
        private float _destinationHudPosY;

        [SerializeField] private HealthBar healthBar;
        [SerializeField] private ManaBar manaBar;
        [SerializeField] private TowerHp towerHp;
        [SerializeField] private TowerMana towerMana;
        [SerializeField] private int playerHealth;
        [SerializeField] private int playerMana;
        [SerializeField] private Image cantMoveImage;

        [field: SerializeField] public Sprite physicalSprite { get; private set; }
        [field: SerializeField] public Sprite magicSprite { get; private set; }
        [field: SerializeField] public Sprite sellSprite { get; private set; }
        [field: SerializeField] public Sprite checkSprite { get; private set; }

#region Unity Event

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _destinationHudPosY = _rectTransform.anchoredPosition.y;
            _hudSlideTween = GetComponent<RectTransform>().DOAnchorPosY(200, 0.3f).From()
                .SetAutoKill(false).Pause().SetUpdate(true);

            var health = healthBar.GetComponent<TowerHealth>();
            health.Init(playerHealth);
            healthBar.Init(health);
            towerHp.Hp = playerHealth;
            towerHp.towerHealth = health;

            var mana = manaBar.GetComponent<Mana>();
            mana.Init(playerMana);
            manaBar.Init(mana);
            towerMana.Mana = playerMana;
            towerMana.towerMana = mana;

            _cantMoveImageSequence = DOTween.Sequence().SetAutoKill(false).Pause()
                .Append(cantMoveImage.transform.DOScale(1, 0.5f).From(0).SetEase(Ease.OutBounce)
                    .SetLoops(2, LoopType.Yoyo))
                .Join(cantMoveImage.DOFade(0, 0.5f).From(1));

        }

        private void OnDisable()
        {
            _hudSlideTween?.Kill();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UIManager.Instance.CameraManager.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var rectAnchorPos = _rectTransform.anchoredPosition;
            rectAnchorPos += new Vector2(0, eventData.delta.y);
            if (rectAnchorPos.y < 0) rectAnchorPos = Vector2.zero;
            else if (rectAnchorPos.y > 160) rectAnchorPos = new Vector2(0, 160);
            _rectTransform.anchoredPosition = rectAnchorPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UIManager.Instance.CameraManager.enabled = true;
            if (_rectTransform.anchoredPosition.y > _rectTransform.rect.height*0.5f)
            {
                //up
                _hudSlideTween.ChangeStartValue(_rectTransform.anchoredPosition)
                    .ChangeEndValue(new Vector2(0, 200))
                    .OnComplete(() => _isHUDVisible = false).Restart();
            }
            else
            {
                //down
                _hudSlideTween.ChangeStartValue(_rectTransform.anchoredPosition)
                    .OnComplete(() => _isHUDVisible = true).Restart();
            }
        }

#endregion

#region Public Method

        public void CannotMoveHere()
        {
            cantMoveImage.transform.position = Input.mousePosition;
            cantMoveImage.enabled = true;
            _cantMoveImageSequence.OnComplete(() => cantMoveImage.enabled = false).Restart();

        }

        public void DisplayHUD()
        {
            if (_isHUDVisible) return;
            _hudSlideTween.ChangeStartValue(_rectTransform.anchoredPosition)
                .ChangeEndValue(new Vector2(0, _destinationHudPosY))
                .OnComplete(() => _isHUDVisible = true).Restart();
        }

#endregion
    }
}