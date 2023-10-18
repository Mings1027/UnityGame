using UnityEngine;

namespace GameControl
{
    public class DayNightCycle : MonoBehaviour
    {
        private Camera _cam;
        private float _dotProduct;
        private bool _isNightStarted;

        [SerializeField] private Transform directionalLight;
        [SerializeField] private AnimationCurve sunCurve;
        [SerializeField] private Color dayTimeColor;
        [SerializeField] private Color sunsetColor;
        [SerializeField] private Color nightTimeColor;

        [Header("==========Background Color==========")] [SerializeField]
        private Color morningColor;

        [SerializeField] private Color afternoonColor;
        [SerializeField] private Color nightColor;

        [SerializeField, Range(0, 100)] private int timeSpeed;
        [SerializeField, Range(0, 1)] private float lerp;

        [SerializeField] private ParticleSystem starParticle;
        [SerializeField] private ParticleSystem cloudParticle;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            RotateCycle();
            // LerpAmbientLight();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void RotateCycle()
        {
            directionalLight.Rotate(Vector3.up * (timeSpeed * Time.deltaTime));
            _dotProduct = Vector3.Dot(directionalLight.forward, Vector3.forward);
        }

        private void LerpAmbientLight()
        {
            if (_dotProduct > 0)
            {
                RenderSettings.ambientLight = Color.Lerp(sunsetColor, dayTimeColor, sunCurve.Evaluate(_dotProduct));
                _cam.backgroundColor = Color.Lerp(morningColor, afternoonColor, sunCurve.Evaluate(_dotProduct));
                lerp = _dotProduct;
                if (!_isNightStarted) return;
                _isNightStarted = false;
                cloudParticle.Play();
                starParticle.Stop();
            }
            else
            {
                var absDotProduct = _dotProduct * -1;
                RenderSettings.ambientLight = Color.Lerp(dayTimeColor, sunsetColor, sunCurve.Evaluate(absDotProduct));
                _cam.backgroundColor = Color.Lerp(afternoonColor, morningColor, sunCurve.Evaluate(absDotProduct));
                lerp = absDotProduct;
                if (_isNightStarted) return;
                _isNightStarted = true;
                starParticle.Play();
                cloudParticle.Stop();
            }
        }
    }
}