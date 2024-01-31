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
        private Image _plusImage;

        [SerializeField] private TMP_Text emeraldAmountText;

        private void Awake()
        {
            _currencyButton = GetComponent<Button>();
            _amountText = transform.GetChild(1).GetComponent<TMP_Text>();
            _plusImage = transform.GetChild(2).GetComponent<Image>();
        }

        public void SetText()
        {
            _amountText.text = BackendGameData.userData.emerald.ToString();
            emeraldAmountText.text = _amountText.text;
        }

        public void On()
        {
            _currencyButton.interactable = true;
            _plusImage.enabled = true;
        }

        public void Off()
        {
            _currencyButton.interactable = false;
            _plusImage.enabled = false;
        }
    }
}