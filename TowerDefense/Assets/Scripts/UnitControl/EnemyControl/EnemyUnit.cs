using System;
using System.Threading;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using DataControl;
using UnityEngine;

namespace UnitControl.EnemyControl
{
    public class EnemyUnit : Unit
    {
        private Vector3 _prevPos;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("BaseTower")) return;
            thisCollider.enabled = false;
            gameObject.SetActive(false);
        }

        public override void UnitUpdate(CancellationTokenSource cts)
        {
            if (!navMeshAgent.enabled) return;
            base.UnitUpdate(cts);
        }

        protected override void Patrol()
        {
            base.Patrol();
            navMeshAgent.SetDestination(Vector3.zero);
        }

        public void SetAnimationSpeed(float animSpeed)
        {
            anim.speed = animSpeed;
        }

        public void SpawnInit(EnemyData enemyData)
        {
            _prevPos = transform.position;
            unitState = UnitState.Patrol;
            navMeshAgent.speed = enemyData.Speed;
            SetAnimationSpeed(navMeshAgent.speed);
            atkDelay = enemyData.AttackDelay;
            damage = enemyData.Damage;
            navMeshAgent.SetDestination(Vector3.zero);
            anim.SetBool(IsWalk, true);
        }

        public async UniTaskVoid IfStuck(CancellationTokenSource cts)
        {
            if (unitState == UnitState.Patrol && Vector3.Distance(_prevPos, transform.position) < 1.5f)
            {
                navMeshAgent.enabled = false;
                await UniTask.Delay(500, cancellationToken: cts.Token);
                if (!gameObject.activeSelf) return;
                navMeshAgent.enabled = true;
                navMeshAgent.ResetPath();
                navMeshAgent.SetDestination(Vector3.zero);
                _prevPos = transform.position;
            }
        }
    }
}