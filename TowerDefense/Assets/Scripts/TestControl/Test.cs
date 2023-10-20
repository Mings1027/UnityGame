using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TestControl
{
    public class Test : MonoBehaviour
    {
        private void Start()
        {
            DOTween.Sequence(transform.DOScale(100, 2)).OnComplete(() =>
            {
                print("its complete");
            });
        }
    }
}