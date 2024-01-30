using BackEnd;
using BackendControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class EmeraldCurrency : MonoBehaviour
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
            // _amountText.text =
            //     Backend.GameData.GetMyData(BackendGameData.UserDataTable, new Where()).FlattenRows()[0]["emerald"]
            //         .ToString();
            _amountText.text = BackendGameData.userData.emerald.ToString();
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