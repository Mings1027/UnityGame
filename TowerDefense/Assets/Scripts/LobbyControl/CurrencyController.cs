using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LobbyControl
{
    public class CurrencyController : MonoBehaviour
    {
        [field: SerializeField] public DiamondCurrency diamondCurrency;
        [field: SerializeField] public EmeraldCurrency emeraldCurrency;

        public void On()
        {
            diamondCurrency.On();
            emeraldCurrency.On();
        }

        public void Off()
        {
            diamondCurrency.Off();
            emeraldCurrency.Off();
        }
    }
}