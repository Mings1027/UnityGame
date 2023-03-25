using System;
using DG.Tweening;
using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    private Camera _cam;
    private Sequence _showSequence, _hideSequence;
    private Transform target;
    private Vector3 pos;
    private bool worldTarget;

    [SerializeField] private Vector3 offset;
    [SerializeField] private Ease scaleEase;
    [SerializeField] private float duration;

    private void Awake()
    {
        _cam = Camera.main;

        _showSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(transform.DOScale(0.5f, duration).From())
            .SetEase(scaleEase);

        _hideSequence = DOTween.Sequence().SetAutoKill(false).Pause()
            .Append(transform.DOScale(0f, duration))
            .SetEase(scaleEase)
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void Update()
    {
        pos = worldTarget ? _cam.WorldToScreenPoint(target.position + offset) : target.position;

        if (transform.position != pos)
        {
            transform.position = pos;
        }
    }

    private void OnDestroy()
    {
        _showSequence.Kill();
    }

    public void WorldTarget(Transform t)
    {
        worldTarget = true;
        target = t;
    }

    public void ScreenTarget(Transform t)
    {
        target = t;
    }

    public void ActivePanel()
    {
        _hideSequence.Pause();
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        _showSequence.Restart();
    }

    public void DeActivePanel()
    {
        _showSequence.Pause();
        _hideSequence.Restart();
    }
}