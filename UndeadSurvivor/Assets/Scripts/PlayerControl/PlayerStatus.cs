using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerControl
{
    public class PlayerStatus : MonoBehaviour
    {
        [SerializeField] private bool _mag;

        [SerializeField] private int maxExp;
        [SerializeField] private int curExp;

        public int CurExp
        {
            get => curExp;
            set
            {
                curExp = value;
                if (curExp < maxExp) return;
                curExp -= maxExp;
                maxExp *= 2;
            }
        }

        public async UniTaskVoid TurnOnMag(int magTime)
        {
            if (_mag) return;
            _mag = true;
            await UniTask.Delay(TimeSpan.FromSeconds(magTime));
            _mag = false;
        }

    }
}