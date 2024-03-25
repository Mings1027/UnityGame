using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Utilities
{
    public class CustomDropdown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        private class DropdownItem
        {
            public string itemName;
            public Action itemSelection;
            public CustomDropDownItem customDropDownItem;
        }

        private CancellationTokenSource _cts;
        private Tween _dropDownTween;
        private bool _isPressed;
        private bool _isExpand;
        private Queue<CustomDropDownItem> _dropdownItemQueue;

        [SerializeField] private DropdownItem[] dropdownItems;
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private Image expandImage;
        [SerializeField] private CanvasGroup dropDownListGroup;
        [SerializeField] private GameObject dropDownItemPrefab;
        [SerializeField, Range(0, 500)] private int dropdownItemWidth;
        [SerializeField, Range(0, 500)] private int dropdownItemHeight;

        private void Start()
        {
            dropDownListGroup.blocksRaycasts = false;
            _dropDownTween = dropDownListGroup.DOFade(1, 0.2f).From(0).SetAutoKill(false).Pause();
            _dropDownTween.OnComplete(() => dropDownListGroup.blocksRaycasts = true);
            _dropDownTween.OnRewind(() => dropDownListGroup.blocksRaycasts = false);

            _dropdownItemQueue = new Queue<CustomDropDownItem>();

            for (var i = 0; i < dropdownItems.Length; i++)
            {
                dropdownItems[i].customDropDownItem = Instantiate(dropDownItemPrefab, dropDownListGroup.transform)
                    .GetComponent<CustomDropDownItem>();
                dropdownItems[i].customDropDownItem.dropDownRect.sizeDelta =
                    new Vector2(dropdownItemWidth, dropdownItemHeight);

                var index = i;
                dropdownItems[i].customDropDownItem.text.text = dropdownItems[index].itemName;
                dropdownItems[i].customDropDownItem.button.onClick.AddListener(() =>
                {
                    if (_dropdownItemQueue.Count > 0)
                    {
                        _dropdownItemQueue.Dequeue().checkImage.enabled = false;
                    }

                    dropdownItems[index].customDropDownItem.checkImage.enabled = true;
                    _dropdownItemQueue.Enqueue(dropdownItems[index].customDropDownItem);

                    mainText.text = dropdownItems[index].customDropDownItem.text.text;

                    LocaleManager.ChangeLocale(index);
                });
            }

            for (var i = 0; i < dropdownItems.Length; i++)
            {
                if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[i])
                {
                    mainText.text = dropdownItems[i].customDropDownItem.text.text;
                    dropdownItems[i].customDropDownItem.checkImage.enabled = true;
                    _dropdownItemQueue.Enqueue(dropdownItems[i].customDropDownItem);
                    break;
                }
            }
        }

        private async UniTaskVoid DropdownUpdate()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Yield();

                if (!_isPressed && Input.GetMouseButtonUp(0) && _isExpand)
                {
                    CloseDropDown();
                }
            }
        }

        private void OpenDropDown()
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            DropdownUpdate().Forget();
            _isExpand = true;
            _dropDownTween.Restart();
            expandImage.rectTransform.localScale = new Vector3(1, -1, 1);
        }

        private void CloseDropDown()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _isExpand = false;
            _dropDownTween.PlayBackwards();
            expandImage.rectTransform.localScale = new Vector3(1, 1, 1);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (_isExpand)
            {
                CloseDropDown();
            }
            else
            {
                OpenDropDown();
            }
        }

        [Conditional("UNITY_EDITOR"), ContextMenu("Add DropdownItem")]
        private void AddDropdownItem()
        {
            Instantiate(dropDownItemPrefab, dropDownListGroup.transform);
        }

        [Conditional("UNITY_EDITOR"), ContextMenu("Remove DropdownItem")]
        private void RemoveDropdownItem()
        {
            var lastChildIndex = dropDownListGroup.transform.childCount - 1;
            DestroyImmediate(dropDownListGroup.transform.GetChild(lastChildIndex).gameObject);
        }
    }
}