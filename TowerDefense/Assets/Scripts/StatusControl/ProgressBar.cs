using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public class ProgressBar : MonoBehaviour
    {
        private Progressive progressive;

        [SerializeField] private Image fillImage;

        private void Awake()
        {
            progressive = GetComponentInParent<Progressive>();
        }

        private void OnEnable()
        {
            fillImage.fillAmount = 1;
            progressive.OnUpdateBar += UpdateBar;
        }

        private void OnDisable() => progressive.OnUpdateBar -= UpdateBar;

        private void UpdateBar() => fillImage.fillAmount = progressive.Ratio;
    }
}