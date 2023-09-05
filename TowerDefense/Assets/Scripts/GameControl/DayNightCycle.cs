using System;
using UnityEngine;

namespace GameControl
{
    public class DayNightCycle : MonoBehaviour
    {
        private Camera _cam;
        private float _dotProduct;
        [SerializeField] private Transform directionalLight;
        [SerializeField] private AnimationCurve sunCurve;
        [SerializeField] private Color dayTimeColor;
        [SerializeField] private Color sunsetColor;
        [SerializeField] private Color nightTimeColor;

        [Header("Background Color")] [SerializeField]
        private Color morningColor;

        [SerializeField] private Color afternoonColor;
        [SerializeField] private Color nightColor;

        [SerializeField] private float timeSpeed;
        [SerializeField, Range(0, 1)] private float lerp;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            RotateCycle();
            LerpAmbientLight();
        }

        private void RotateCycle()
        {
            directionalLight.RotateAround(Vector3.zero, directionalLight.right, Time.deltaTime * timeSpeed);
            _dotProduct = Vector3.Dot(directionalLight.forward, Vector3.down);
        }

        private void LerpAmbientLight()
        {
            if (_dotProduct > 0)
            {
                RenderSettings.ambientLight = Color.Lerp(sunsetColor, dayTimeColor, sunCurve.Evaluate(_dotProduct));
                _cam.backgroundColor = Color.Lerp(morningColor, afternoonColor, sunCurve.Evaluate(_dotProduct));
                lerp = _dotProduct;
            }
            else
            {
                var absDotProduct = _dotProduct * -1;
                RenderSettings.ambientLight = Color.Lerp(sunsetColor, nightTimeColor, sunCurve.Evaluate(absDotProduct));
                _cam.backgroundColor = Color.Lerp(morningColor,nightColor, sunCurve.Evaluate(absDotProduct));
                lerp = absDotProduct;
            }


        }

        // private void OnDrawGizmos()
        // {
        //     var c = RenderSettings.ambientLight;
        //     c.a = 255;
        //     Gizmos.color = c;
        //     Gizmos.DrawSphere(Vector3.zero, 5);
        // }
    }
}