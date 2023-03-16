using System;
using DG.Tweening;
using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    private Camera _cam;
    private Sequence _showSequence, _hideSequence;

    public Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Ease scaleEase;
    [SerializeField] private float duration;

    private void Awake()
    {
        _cam = Camera.main;

        _showSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(transform.DOScale(0, duration).From())
            .SetEase(scaleEase);

        _hideSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(transform.DOScale(0, duration))
            .SetEase(scaleEase);
    }

    private void Update()
    {
        var pos = _cam.WorldToScreenPoint(target.position + offset);

        if (transform.position != pos)
        {
            transform.position = pos;
        }
    }

    private void OnDestroy()
    {
        _showSequence.Kill();
    }

    public void ActivePanel()
    {
        _hideSequence.Pause();
        _showSequence.Restart();
    }

    public void DeActivePanel()
    {
        _showSequence.Pause();
        _hideSequence.Restart();
    }
}