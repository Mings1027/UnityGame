using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class HealthBar : MonoBehaviour
    {
        private Camera _cam;
        [SerializeField] private Image healthBarForeground;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void OnEnable()
        {
            healthBarForeground.fillAmount = 1;
        }

        private void LateUpdate()
        {
            transform.LookAt(_cam.transform,Vector3.up);
        }

        public void UpdateHealthBar(float amount)
        {
            healthBarForeground.fillAmount = amount;
        }
    }
}