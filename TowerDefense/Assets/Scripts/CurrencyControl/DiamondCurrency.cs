using System;
using BackEnd;
using BackendControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurrencyControl
{
    public class DiamondCurrency : MonoBehaviour
    {
        private Button _currencyButton;
        private TMP_Text _amountText;

        [SerializeField] private TMP_Text diaAmountText;

        private void Awake()
        {
            _currencyButton = GetComponent<Button>();
            _amountText = transform.GetChild(1).GetComponent<TMP_Text>();
        }

        private void Start()
        {
            TextInit();
        }

        public void TextInit()
        {
            var bro = Backend.TBC.GetTBC();
            if (bro.IsSuccess())
            {
                BackendGameData.curTbc = int.Parse(bro.GetReturnValuetoJSON()["amountTBC"].ToString());
            }

            SetText();
        }

        public void SetText()
        {
            _amountText.text = BackendGameData.curTbc.ToString();
            diaAmountText.text = _amountText.text;
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