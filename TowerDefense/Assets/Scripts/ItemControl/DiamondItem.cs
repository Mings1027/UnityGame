using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemControl
{
    public class DiamondItem : MonoBehaviour
    {
        public Button purchaseButton { get; private set; }

        [field: SerializeField] public string productId { get; private set; }
        [field: SerializeField] public TMP_Text priceText { get; private set; }

        private void Awake()
        {
            purchaseButton = GetComponent<Button>();
        }
    }
}