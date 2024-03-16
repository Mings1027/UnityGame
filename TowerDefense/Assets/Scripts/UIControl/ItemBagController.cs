using System;
using System.Collections.Generic;
using System.Linq;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ItemControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utilities;

namespace UIControl
{
    public class ItemBagController : MonoBehaviour
    {
        private TowerCardController _towerCardController;
        private Tween _disappearItemBagTween;
        private Tween _itemBagTween;
        private Tween _descriptionTween;
        private Dictionary<ItemType, string> _itemDescriptionDic;
        private Dictionary<ItemType, ItemButton> _itemDic;
        private Dictionary<ItemType, int> _itemCountDic;
        private ItemType _curItemType;
        private bool _isOpenBag;
        private bool _isOpenDescription;
        public static bool isOnItemButton { get; set; }

        [SerializeField] private Button bagButton;
        [SerializeField] private CanvasGroup itemBagGroup;
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Image selectIcon;

        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup descriptionGroup;
        [SerializeField] private TMP_Text itemDescriptionText;

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += ChangeItemDescLocale;
        }

        private void Start()
        {
            Init();
            TweenInit();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeItemDescLocale;
        }

        private void OnDestroy()
        {
            _itemBagTween?.Kill();
        }

        private void Init()
        {
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

            _itemDic = new Dictionary<ItemType, ItemButton>();
            _itemCountDic = new Dictionary<ItemType, int>();
            _itemDescriptionDic = new Dictionary<ItemType, string>();
            var itemInventory = BackendGameData.userData.itemInventory;

            for (var i = 0; i < itemGroup.transform.childCount; i++)
            {
                var itemButton = itemGroup.transform.GetChild(i).GetComponent<ItemButton>();
                _itemDic.Add(itemButton.itemType, itemButton);
                CustomLog.Log(itemButton.itemType);
                itemButton.OnSetCurItemEvent += SetCurItem;
                itemButton.OnDisplayItemDescEvent += () => DisplayItemDescription().Forget();
                itemButton.OnClickItemEvent += ClickItem;
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);

                _itemCountDic.Add(itemButton.itemType, itemInventory[itemButton.itemType.ToString()]);
                _itemDescriptionDic.Add(itemButton.itemType,
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemButton.itemType.ToString()));
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
            _descriptionTween.OnComplete(() =>
            {
                descriptionGroup.blocksRaycasts = true;
            });
            descriptionGroup.blocksRaycasts = false;
        }

        private void ChangeItemDescLocale(Locale locale)
        {
            foreach (var itemType in _itemDescriptionDic.Keys.ToList())
            {
                _itemDescriptionDic[itemType] =
                    LocaleManager.GetLocalizedString(LocaleManager.ItemTable, itemType.ToString());
            }
        }

        private void SetCurItem(ItemType curItemType)
        {
            _curItemType = curItemType;
        }

        private async UniTaskVoid DisplayItemDescription()
        {
            CloseDescription();
            
            await UniTask.Delay(500);
            if (isOnItemButton)
            {
                _isOpenDescription = true;
                SoundManager.PlayUISound(SoundEnum.ButtonSound);
                selectIcon.gameObject.SetActive(false);
                itemDescriptionText.text = _itemDescriptionDic[_curItemType];
                _descriptionTween?.Restart();
            }
        }

        private void ClickItem(Vector2 pos)
        {
            if (!_isOpenBag) return;
            if (_isOpenDescription) return;
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            selectIcon.gameObject.SetActive(true);
            selectIcon.rectTransform.anchoredPosition = pos;
        }

        private void UseItem()
        {
            SoundManager.PlayUISound(SoundEnum.ButtonSound);
            if (_itemCountDic[_curItemType] <= 0) return;
            _itemDic[_curItemType].Spawn();
            _itemDic[_curItemType].DecreaseItemCount();
            selectIcon.gameObject.SetActive(false);
            _itemCountDic[_curItemType] -= 1;
            _curItemType = ItemType.None;
        }

        private void CloseDescription()
        {
            _isOpenDescription = false;
            descriptionGroup.blocksRaycasts = false;
            _descriptionTween?.PlayBackwards();
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
            _curItemType = ItemType.None;
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
                if (itemType == ItemType.None) continue;
                BackendGameData.userData.itemInventory[itemType.ToString()] = _itemCountDic[itemType];
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