using System;
using System.Collections.Generic;
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
            public UnityEvent itemSelection;
            public CustomDropDownItem customDropDownItem;
        }

        private Tween _dropDownTween;
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
                dropdownItems[i].customDropDownItem.button.onClick
                    .AddListener(() =>
                    {
                        if (_dropdownItemQueue.Count > 0)
                        {
                            _dropdownItemQueue.Dequeue().checkImage.enabled = false;
                        }

                        dropdownItems[index].itemSelection?.Invoke();
                        dropdownItems[index].customDropDownItem.checkImage.enabled = true;
                        _dropdownItemQueue.Enqueue(dropdownItems[index].customDropDownItem);

                        mainText.text = dropdownItems[index].customDropDownItem.text.text;

                        _isExpand = false;
                        _dropDownTween.PlayBackwards();
                        expandImage.rectTransform.localScale = new Vector3(1, 1, 1);

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

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isExpand)
            {
                _isExpand = false;
                _dropDownTween.PlayBackwards();
                expandImage.rectTransform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                _isExpand = true;
                _dropDownTween.Restart();
                expandImage.rectTransform.localScale = new Vector3(1, -1, 1);
            }
        }
    }
}