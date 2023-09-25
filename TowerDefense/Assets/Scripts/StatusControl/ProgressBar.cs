using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace StatusControl
{
    public class ProgressBar : MonoBehaviour
    {
        private Progressive _progressive;
        private Transform border;

        [SerializeField] private Image fillImage;

        [SerializeField] private float duration;
        [SerializeField] private float strength;
        [SerializeField] private int vibrato;

        private void Awake()
        {
            _progressive = GetComponentInParent<Progressive>();
            border = transform.GetChild(0);
        }

        private void OnEnable()
        {
            fillImage.fillAmount = 1;
            _progressive.OnUpdateBarEvent += UpdateBarEvent;
            _progressive.OnUpdateBarEvent += ShakeBarEvent;
        }

        private void OnDisable()
        {
            _progressive.OnUpdateBarEvent -= UpdateBarEvent;
            _progressive.OnUpdateBarEvent -= ShakeBarEvent;
        }

        private void UpdateBarEvent() => fillImage.fillAmount = _progressive.Ratio;

        private void ShakeBarEvent() => border.DOShakeScale(duration, strength, vibrato)
            .OnComplete(() => border.localScale = Vector3.one);
    }
}