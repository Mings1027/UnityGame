using BackEnd;
using BackendControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class DiamondCurrency : MonoBehaviour
    {
        private Button _currencyButton;
        private TMP_Text _amountText;
        private Image _plusImage;

        private void Awake()
        {
            _currencyButton = GetComponent<Button>();
            _amountText = transform.GetChild(1).GetComponent<TMP_Text>();
            _plusImage = transform.GetChild(2).GetComponent<Image>();
        }

        public void SetText()
        {
            var bro = Backend.TBC.GetTBC();
            if (bro.IsSuccess())
            {
                var amountTbc = int.Parse(bro.GetReturnValuetoJSON()["amountTBC"].ToString());
                _amountText.text = amountTbc.ToString();
                BackendGameData.curTbc = amountTbc;
            }
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