using System.Collections.Generic;
using BackendControl;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class ItemBagController : MonoBehaviour
    {
        private Dictionary<ItemType, ItemButton> _itemDic;
        private ItemType _curItemType;
        private Button _button;
        private bool _itemBagActive;
        private bool _isSelectedItem;
        [SerializeField] private GameObject itemParent;
        [SerializeField] private Image selectIcon;
        [SerializeField] private ItemButton[] itemButtons;

        private void Start()
        {
            _button = GetComponent<Button>();
            itemParent.SetActive(false);
            selectIcon.enabled = false;
            _button.onClick.AddListener(() =>
            {
                if (_itemBagActive)
                {
                    selectIcon.enabled = false;
                    _isSelectedItem = false;
                    _curItemType = ItemType.None;
                    _itemBagActive = false;
                    itemParent.SetActive(false);
                }
                else
                {
                    _itemBagActive = true;
                    itemParent.SetActive(true);
                }
            });
            _itemDic = new Dictionary<ItemType, ItemButton>();
            var itemInventory = BackendGameData.userData.itemInventory;
            for (var i = 0; i < itemButtons.Length; i++)
            {
                var itemButton = itemButtons[i].GetComponent<ItemButton>();
                _itemDic.Add(itemButton.itemType, itemButton);
                itemButton.OnSetCurItemEvent += SetCurItem;
                itemButton.SetRemainingText(itemInventory[itemButton.itemType.ToString()]);
            }
        }

        private void SetCurItem(ItemType itemType, Vector2 anchoredPos)
        {
            if (_isSelectedItem) return;
            Debug.Log($"아이템 누름 : {itemType}");
            if (!selectIcon.enabled) selectIcon.enabled = true;
            selectIcon.rectTransform.anchoredPosition = anchoredPos;
            _isSelectedItem = true;
            _curItemType = itemType;
            WaitForTouch().Forget();
        }

        private async UniTaskVoid WaitForTouch()
        {
            while (Input.touchCount <= 0)
            {
                Debug.Log("기다리는 중");
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
                if (Input.touchCount != 1) continue;
                UseItem();
                _isSelectedItem = false;
                break;
            }
        }

        private void UseItem()
        {
            _itemDic[_curItemType].Spawn();
            _itemDic[_curItemType].DecreaseItemCount();
        }
    }
}