using System;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public abstract class EnemyUnit : Unit
    {
        
        public int Number { get; set; }
        public Transform destination;

        public event Action<int> onFinishWaveCheckEvent;

        private void Update()
        {
            if (gameManager.IsPause) return;
            if (isTargeting)
            {
                if (nav.remainingDistance <= nav.stoppingDistance)
                {
                    if (!attackAble) return;
                    nav.isStopped = true;
                    Attack();
                    StartCoolDown();
                }
                else
                {
                    nav.SetDestination(target.position);
                }
            }
            else
            {
                if (nav.isStopped) nav.isStopped = false;
                nav.SetDestination(destination.position);
                if (Vector3.Distance(transform.position, destination.position) <= nav.stoppingDistance)
                    gameObject.SetActive(false);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onFinishWaveCheckEvent?.Invoke(Number);
            onFinishWaveCheckEvent = null;
        }
    }
}