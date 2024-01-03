using Cysharp.Threading.Tasks;
using DataControl.MonsterDataControl;
using UnityEngine;

namespace MonsterControl
{
    public class FlyingMonster : MonsterUnit
    {
        [SerializeField] private byte baseOffset;

        // protected override void Awake()
        // {
        //     base.Awake();
        //     targetLayer = LayerMask.GetMask("Tower");
        //     targetCollider = new Collider[1];
        // }

        // public override void Init()
        // {
        //     base.Init();
        //     patrolCooldown.cooldownTime = 3f;
        // }

        public override void SpawnInit(MonsterData monsterData)
        {
            base.SpawnInit(monsterData);
            SetBaseOffset().Forget();
        }

        private async UniTaskVoid SetBaseOffset()
        {
            if (baseOffset == 0) return;
            var lerp = 0f;
            while (navMeshAgent.baseOffset < baseOffset)
            {
                await UniTask.Delay(10);
                lerp += Time.deltaTime;
                var offset = Mathf.Lerp(0, baseOffset, lerp);
                navMeshAgent.baseOffset = offset;
            }
        }

        public override void MonsterUpdate()
        {
            var pos = transform.position;
            pos.y = 0;
            if (Vector3.Distance(pos, Vector3.zero) <= 0.5f)
            {
                DisableObject();
            }
        }
        // public override void MonsterUpdate()
        // {
        //     if (!navMeshAgent.enabled) return;
        //     if (health.IsDead) return;
        //     switch (unitState)
        //     {
        //         case UnitState.Patrol:
        //             Patrol();
        //             break;
        //         case UnitState.Attack:
        //             Attack();
        //             break;
        //     }
        //
        //     base.MonsterUpdate();
        // }

        // protected override void Patrol()
        // {
        //     var pos = transform.position;
        //     pos.y = 0;
        //     if (Vector3.Distance(pos, Vector3.zero) <= 0.5f)
        //     {
        //         UIManager.Instance.BaseTowerHealth.Damage(baseTowerDamage);
        //         DisableObject();
        //         return;
        //     }
        //
        //     if (nonAttackableMonster) return;
        //     
        //     var size = Physics.OverlapSphereNonAlloc(transform.position, sightRange, targetCollider, targetLayer);
        //     if (size <= 0)
        //     {
        //         target = null;
        //         if (navMeshAgent.isOnNavMesh)
        //         {
        //             navMeshAgent.SetDestination(Vector3.zero);
        //         }
        //
        //         return;
        //     }
        //
        //     if (!target || !target.enabled)
        //     {
        //         target = targetCollider[0];
        //     }
        //
        //     unitState = UnitState.Attack;
        // }

        // protected override void Attack()
        // {
        //     if (attackCooldown.IsCoolingDown) return;
        //     var t = transform;
        //     var targetRot = Quaternion.LookRotation(target.transform.position - t.position);
        //     t.rotation = Quaternion.Slerp(t.rotation, targetRot, turnSpeed);
        //
        //     TryDamage();
        //     unitState = UnitState.Patrol;
        //     attackCooldown.StartCooldown();
        // }
        //
        // protected override void TryDamage()
        // {
        // }
    }
}