using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DataControl;
using DG.Tweening;
using TowerControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyStatus : MonoBehaviour
    {
        private EnemyAI _enemyAI;
        private EnemyHealth _enemyHealth;
        private bool _isSlowed;
        private float _defaultSpeed;

        [SerializeField] private float slowImmunityTime;

        private void Awake()
        {
            _enemyAI = GetComponent<EnemyAI>();
            _defaultSpeed = _enemyAI.MoveSpeed;
        }

        private void OnEnable()
        {
            StatusInit();
        }

        private void StatusInit()
        {
            _isSlowed = false;
            _enemyAI.MoveSpeed = _defaultSpeed;
        }

        public void SlowEffect(ref DeBuffData.SpeedDeBuffData speedDeBuffData)
        {
            if (_isSlowed) return;
            _isSlowed = true;
            _enemyAI.MoveSpeed -= speedDeBuffData.decreaseSpeed;
            if (_enemyAI.MoveSpeed < 0.5f) _enemyAI.MoveSpeed = 0.5f;
            SlowEffectTween(speedDeBuffData);
        }

        private void SlowEffectTween(DeBuffData.SpeedDeBuffData speedDeBuff)
        {
            DOVirtual.DelayedCall(speedDeBuff.deBuffTime, () => _enemyAI.MoveSpeed += speedDeBuff.decreaseSpeed)
                .OnComplete(
                    () => { DOVirtual.DelayedCall(slowImmunityTime, () => _isSlowed = false); });
        }

        public void ContinuousDamage()
        {
            _enemyHealth.Damage(1);
        }
    }
}