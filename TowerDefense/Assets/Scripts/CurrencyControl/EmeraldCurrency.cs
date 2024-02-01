using System;
using BackendControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurrencyControl
{
    public class EmeraldCurrency : MonoBehaviour
    {
        private Button _currencyButton;
        private TMP_Text _amountText;

        [SerializeField] private TMP_Text emeraldAmountText;

        private void Awake()
        {
            _currencyButton = GetComponent<Button>();
            _amountText = transform.GetChild(1).GetComponent<TMP_Text>();
        }

        private void Start()
        {
            SetText();
        }

        public void SetText()
        {
            _amountText.text = BackendGameData.userData.emerald.ToString();
            emeraldAmountText.text = _amountText.text;
        }

        public void On()
        {
            _currencyButton.interactable = true;
        }

        public void Off()
        {
            _currencyButton.interactable = false;
        }
    }
}