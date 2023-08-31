using System;
using UnityEngine;

namespace GameControl
{
    public class TimeController : MonoBehaviour
    {
        private DateTime _currentTime;
        private TimeSpan _sunriseTime;
        private TimeSpan _sunsetTime;

        [SerializeField] private float timeMultiplier;
        [SerializeField] private float startHour;

        [SerializeField] private float sunriseHour;
        [SerializeField] private float sunsetHour;

        [SerializeField] private AnimationCurve lightChangeCurve;
        
        [SerializeField] private Color dayAmbientLight;
        [SerializeField] private Color nightAmbientLight;

        [SerializeField] private float maxSunLightIntensity;
        [SerializeField] private float maxMoonLightIntensity;

        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;


        private void Start()
        {
            _currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
            _sunriseTime = TimeSpan.FromHours(sunriseHour);
            _sunsetTime = TimeSpan.FromHours(sunsetHour);
        }

        private void Update()
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
        }

        private void UpdateTimeOfDay()
        {
            _currentTime = _currentTime.AddSeconds(Time.deltaTime * timeMultiplier);
        }

        private void RotateSun()
        {
            float sunLightRotation;
            if (_currentTime.TimeOfDay > _sunriseTime && _currentTime.TimeOfDay < _sunsetTime)
            {
                var sunriseToSunsetDuration = CalculateTimeDifference(_sunriseTime, _sunsetTime);
                var timeSinceSunrise = CalculateTimeDifference(_sunriseTime, _currentTime.TimeOfDay);

                var percentage = timeSinceSunrise.TotalSeconds / sunriseToSunsetDuration.TotalSeconds;
                sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
            }
            else
            {
                var sunsetToSunriseDuration = CalculateTimeDifference(_sunsetTime, _sunriseTime);
                var timeSinceSunset = CalculateTimeDifference(_sunsetTime, _currentTime.TimeOfDay);

                var percentage = timeSinceSunset.TotalSeconds / sunsetToSunriseDuration.TotalSeconds;
                sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
            }

            sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
        }

        private void UpdateLightSettings()
        {
            var dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
            sunLight.intensity = Mathf.Lerp(0, maxSunLightIntensity, lightChangeCurve.Evaluate(dotProduct));
            moonLight.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, lightChangeCurve.Evaluate(dotProduct));
            RenderSettings.ambientLight= Color.Lerp(nightAmbientLight,dayAmbientLight,lightChangeCurve.Evaluate(dotProduct));
        }

        private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
        {
            var difference = toTime - fromTime;
            if (difference.TotalSeconds < 0)
            {
                difference += TimeSpan.FromHours(24);
            }

            return difference;
        }
    }
}