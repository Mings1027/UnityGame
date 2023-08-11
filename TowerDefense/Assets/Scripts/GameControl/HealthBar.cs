using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class HealthBar : MonoBehaviour
    {
        private Quaternion _previousRotation;

        [SerializeField] private Image healthBarForeground;

        private void OnEnable()
        {
            healthBarForeground.fillAmount = 1;
        }

        public void UpdateHealthBar(float amount)
        {
            healthBarForeground.fillAmount = amount;
        }
    }
}