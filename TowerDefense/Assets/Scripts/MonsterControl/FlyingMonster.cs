using CustomEnumControl;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterControl
{
    public class FlyingMonster : MonsterUnit
    {
        private Cooldown _moveAroundCooldown;

        [SerializeField] private byte baseOffset;

        protected override void Awake()
        {
            base.Awake();
            _moveAroundCooldown.cooldownTime = 0.5f;
        }

        public override void SpawnInit()
        {
            base.SpawnInit();
            SetBaseOffset().Forget();
        }

        private async UniTaskVoid SetBaseOffset()
        {
            if (baseOffset == 0) return;
            var linearValue = 0f;
            while (navMeshAgent.baseOffset < baseOffset)
            {
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                linearValue += Time.deltaTime;
                var offset = Mathf.Lerp(0, baseOffset, linearValue);
                navMeshAgent.baseOffset = offset;
            }
        }

        protected override void Attack()
        {
            var position = transform.position;
            var checkPos = new Vector3(position.x, 0, position.z);

            if (!target || !target.enabled ||
                Vector3.Distance(target.transform.position, checkPos) > monsterData.attackRange)
            {
                unitState = UnitState.Patrol;
                return;
            }

            if (!_moveAroundCooldown.IsCoolingDown)
            {
                var ranDir = Random.insideUnitSphere * monsterData.attackRange;
                ranDir += transform.position;
                NavMesh.SamplePosition(ranDir, out var hit, 10, NavMesh.AllAreas);
                var finalPos = hit.position;
                navMeshAgent.SetDestination(finalPos);
                _moveAroundCooldown.StartCooldown();
            }

            if (attackCooldown.IsCoolingDown) return;
            anim.SetTrigger(isAttack);
            TryDamage();
            attackCooldown.StartCooldown();
        }
    }
}