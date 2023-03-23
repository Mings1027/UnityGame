using System;
using DG.Tweening;
using UnityEngine;


public class TestTower : MonoBehaviour
{
    private Tween _delayTween;
    private bool _attackAble;
    [SerializeField] private float delay;

    private void Awake()
    {
        _delayTween = DOVirtual.DelayedCall(delay, () => _attackAble = true).SetAutoKill(false);
    }

    private void Update()
    {
        if (_attackAble)
        {
            Debug.Log("attttttttttttack");
            _attackAble = false;
            _delayTween.Restart();
        }
    }
}