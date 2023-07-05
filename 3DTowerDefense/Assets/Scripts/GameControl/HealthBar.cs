using System;
using Unity.Mathematics;
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

        // private void LateUpdate()
        // {
        //     if (_cam.transform.rotation == transform.rotation) return;
        //     
        //     var rotation = _cam.transform.rotation;
        //     transform.rotation = rotation;
        // }

        private void FourDirection()
        {
            if (_cam.transform.rotation == new Quaternion(45, 0, 0, 1))
            {
                transform.rotation = new Quaternion(45, 0, 0, 1);
            }
            else if (_cam.transform.rotation == new Quaternion(45, 90, 0, 1))
            {
                transform.rotation = new Quaternion(45, 90, 0, 1);
            }
            else if (_cam.transform.rotation == new Quaternion(45, 180, 0, 1))
            {
                transform.rotation = new quaternion(45, 180, 0, 1);
            }
            else if (_cam.transform.rotation == new Quaternion(45, 270, 0, 1))
            {
                transform.rotation = new Quaternion(45, 270, 0, 1);
            }
        }
        
        public void UpdateHealthBar(float amount)
        {
            healthBarForeground.fillAmount = amount;
        }
    }
}