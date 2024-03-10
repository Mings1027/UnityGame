using UnityEngine;

namespace GameControl
{
    public class LightColorController : MonoBehaviour
    {
        private float _elapsedTime;
        private float _durationInverse;
        [SerializeField] private Color startColor;
        [SerializeField] private Color endColor;
        [SerializeField] private float duration;

        private void Start()
        {
            _durationInverse = 1f / duration;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > duration)
            {
                _elapsedTime = 0f;
                (startColor, endColor) = (endColor, startColor);
            }

            var t = _elapsedTime * _durationInverse;
            RenderSettings.ambientLight = Color.Lerp(startColor, endColor, t);
        }
    }
}