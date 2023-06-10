using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl
{
    public class HealthBar : MonoBehaviour
    {
        private Camera _cam;
        private Quaternion _previousRotation;
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
            if (_cam.transform.rotation == _previousRotation) return;

            var rotation = _cam.transform.rotation;
            transform.rotation = rotation;
            _previousRotation = rotation;
        }

        public void UpdateHealthBar(float amount)
        {
            healthBarForeground.fillAmount = amount;
        }
    }
}