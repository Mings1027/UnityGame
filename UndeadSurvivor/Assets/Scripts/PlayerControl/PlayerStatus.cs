using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerControl
{
    public class PlayerStatus : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int curHealth;

        public int CurHealth
        {
            get => curHealth;
            set
            {
                curHealth = value;
                if (curHealth >= maxHealth) curHealth = maxHealth;
            }
        }

        [SerializeField] private UnityEvent<GameObject> onHitWithReference, onDeathWithReference;
        private bool _isLive;
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

        private void Start()
        {
            _isLive = true;
            curHealth = maxHealth;
        }

        public async UniTaskVoid TurnOnMag(int magTime)
        {
            if (_mag) return;
            _mag = true;
            await UniTask.Delay(TimeSpan.FromSeconds(magTime));
            _mag = false;
        }

        public void GetHit(int amount, GameObject sender)
        {
            if (!_isLive) return;
            curHealth -= amount;
            if (curHealth > 0)
            {
                onHitWithReference?.Invoke(sender);
            }
            else
            {
                onDeathWithReference?.Invoke(sender);
                _isLive = false;
                gameObject.SetActive(false);
            }
        }
    }
}