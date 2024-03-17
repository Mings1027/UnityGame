using System;
using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ItemControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class ItemBagController : MonoBehaviour
    {
        private DataManager _dataManager;
        private TowerCardController _towerCardController;
        private Tween _disappearItemBagTween;
        private Tween _itemBagTween;
        private Tween _descriptionTween;
        private Dictionary<ItemType, ItemButton> _itemButtonTable;
        private ItemType _curItemType;
        private bool _isOpenBag;
        private bool _isOpenDescription;
        private bool _isPointerDown;
        private bool _isClicking;
        private float _clickDuration;

        [SerializeField] private Button bagButton;
        [SerializeField] private CanvasGroup itemBagGroup;
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Image selectIcon;

        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup descriptionGroup;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescriptionText;

        private void Start()
        {
            Init();
            TweenInit();
        }

        private void OnDestroy()
        {
            _itemBagTween?.Kill();
        }

        private void Init()
        {
            _dataManager = FindAnyObjectByType<DataManager>();
            _towerCardController = FindAnyObjectByType<TowerCardController>();
            selectIcon.GetComponent<Button>().onClick.AddListener(UseItem);
            selectIcon.gameObject.SetActive(false);
            closeButton.onClick.AddListener(CloseDescription);
            bagButton.onClick.AddListener(() =>
            {
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                if (_isOpenBag)
                {
                    _isOpenBag = false;
                    CloseBag();
                }
                else
                {
                    _isOpenBag = true;
                    OpenBag();
                }
            });

            _itemButtonTable = new Dictionary<ItemType, ItemButton>();
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemGroup.transform.childCount; i++)
            {
                var itemButton = itemGroup.transform.GetChild(i).GetComponent<ItemButton>();
                itemButton.OnPointerDownEvent += PointerDown;
                itemButton.OnPointerUpEvent += PointerUp;
                itemButton.OnClickEvent += ClickItem;
                _itemButtonTable.Add(itemButton.itemType, itemButton);
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);
            }
        }

        private void TweenInit()
        {
            _disappearItemBagTween = GetComponent<CanvasGroup>().DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _disappearItemBagTween.OnComplete(() => itemBagGroup.blocksRaycasts = true);
            itemBagGroup.blocksRaycasts = false;

            _itemBagTween = itemGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _itemBagTween.OnComplete(() => itemGroup.blocksRaycasts = true);
            itemGroup.blocksRaycasts = false;

            _descriptionTween = descriptionGroup.DOFade(1, 0.25f).From(0).SetAutoKill(false).Pause();
            _descriptionTween.OnComplete(() => { descriptionGroup.blocksRaycasts = true; });
            descriptionGroup.blocksRaycasts = false;
        }

        private void PointerDown()
        {
            _isPointerDown = true;
            _clickDuration = 0f;
            CloseDescription();
            CheckClicking().Forget();
        }

        private async UniTaskVoid CheckClicking()
        {
            _isClicking = true;
            while (_isClicking)
            {
                await UniTask.Yield();
                _clickDuration += Time.deltaTime;
                if (_clickDuration < 0.5f) continue;
                _isClicking = false;
                DisplayItemDescription();
                break;
            }
        }

        private void PointerUp()
        {
            _isPointerDown = false;
            _isClicking = false;
        }

        private void ClickItem(ItemType curItemType, Vector2 pos)
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_isOpenDescription) return;
            _curItemType = curItemType;
            if (_dataManager.itemInfoTable[_curItemType].itemCount <= 0) return;
            selectIcon.gameObject.SetActive(true);
            selectIcon.rectTransform.anchoredPosition = pos;
        }

        private void DisplayItemDescription()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _isOpenDescription = true;
            selectIcon.gameObject.SetActive(false);
            itemNameText.text = _dataManager.itemInfoTable[_curItemType].itemName;
            itemDescriptionText.text = _dataManager.itemInfoTable[_curItemType].itemDescription;
            _descriptionTween?.Restart();
        }

        private void CloseDescription()
        {
            _isOpenDescription = false;
            descriptionGroup.blocksRaycasts = false;
            _descriptionTween?.PlayBackwards();
        }

        private void UseItem()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            _itemButtonTable[_curItemType].Spawn();
            _itemButtonTable[_curItemType].DecreaseItemCount();
            selectIcon.gameObject.SetActive(false);
            _dataManager.itemInfoTable[_curItemType].itemCount -= 1;
        }

        private void OpenBag()
        {
            _itemBagTween.Restart();
            if (!SafeArea.activeSafeArea)
            {
                _towerCardController.scaleTween.Restart();
            }
        }

        private void CloseBag()
        {
            selectIcon.gameObject.SetActive(false);
            _itemBagTween.PlayBackwards();
            if (!SafeArea.activeSafeArea)
            {
                _towerCardController.scaleTween.PlayBackwards();
            }
        }

        public void UpdateInventory()
        {
            var itemTypes = Enum.GetValues(typeof(ItemType));
            foreach (ItemType itemType in itemTypes)
            {
                BackendGameData.userData.itemInventory[itemType.ToString()] =
                    _dataManager.itemInfoTable[itemType].itemCount;
            }
        }

        public void SetActiveItemBag(bool active)
        {
            if (active)
            {
                _disappearItemBagTween.Restart();
            }
            else
            {
                _isOpenBag = false;
                itemBagGroup.blocksRaycasts = false;
                itemGroup.blocksRaycasts = false;
                _disappearItemBagTween.PlayBackwards();
                CloseBag();
                CloseDescription();
                if (!SafeArea.activeSafeArea)
                {
                    _towerCardController.scaleTween.PlayBackwards();
                }
            }
        }
    }
}