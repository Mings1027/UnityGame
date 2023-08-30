using DG.Tweening;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    private Light _light;
    [SerializeField] private Color nightColor;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    private void Start()
    {
        transform.DORotate(new Vector3(30, 360, 0), 3, RotateMode.FastBeyond360)
            .OnStart(() => transform.rotation = Quaternion.Euler(30, -30, 0))
            .SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        _light.DOColor(nightColor, 3).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
    }
}