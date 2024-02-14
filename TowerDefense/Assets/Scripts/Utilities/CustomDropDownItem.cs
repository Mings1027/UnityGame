using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class CustomDropDownItem : MonoBehaviour
    {
        public Button button { get; private set; }
        public TMP_Text text { get; private set; }
        public RectTransform dropDownRect { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            text = GetComponentInChildren<TMP_Text>();
            dropDownRect = GetComponent<RectTransform>();
        }
    }
}