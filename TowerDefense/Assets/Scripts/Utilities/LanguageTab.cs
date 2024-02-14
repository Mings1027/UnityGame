using CustomEnumControl;
using ManagerControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class LanguageTab : MonoBehaviour
    {
        private bool _isOpen;
        private CustomDropDownItem[] _customDropDownItems;

        [SerializeField] private Button dropDownButton;
        [SerializeField] private TMP_Text dropDownMainTxt;
        [SerializeField] private GameObject dropDownList;
        [SerializeField] private Image checkImage;

        private void Start()
        {
            dropDownButton.onClick.AddListener(() =>
            {
                if (!_isOpen)
                {
                    _isOpen = true;
                    OpenDropDown();
                }
                else
                {
                    _isOpen = false;
                    CloseDropDown();
                }
            });
            _customDropDownItems = new CustomDropDownItem[dropDownList.transform.childCount];
            for (int i = 0; i < _customDropDownItems.Length; i++)
            {
                var index = i;
                _customDropDownItems[i] = dropDownList.transform.GetChild(i).GetComponent<CustomDropDownItem>();
                _customDropDownItems[i].button.onClick.AddListener(() =>
                {
                    SoundManager.PlayUISound(SoundEnum.ButtonSound);
                    dropDownMainTxt.text = _customDropDownItems[index].text.text;
                    LocaleManager.ChangeLocale(index);
                    checkImage.rectTransform.anchoredPosition =
                        _customDropDownItems[index].dropDownRect.anchoredPosition + new Vector2(-250, 30);
                });
            }

            dropDownList.SetActive(false);
            checkImage.enabled = false;
        }

        private void OpenDropDown()
        {
            dropDownList.SetActive(true);
            checkImage.enabled = true;
        }

        private void CloseDropDown()
        {
            dropDownList.SetActive(false);
            checkImage.enabled = false;
        }
    }
}