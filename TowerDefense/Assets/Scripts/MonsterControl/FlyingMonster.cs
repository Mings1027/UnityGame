using Cysharp.Threading.Tasks;
using DataControl.MonsterDataControl;
using UnityEngine;

namespace MonsterControl
{
    public class FlyingMonster : MonsterUnit
    {
        [SerializeField] private byte baseOffset;

        public override void SpawnInit(MonsterData monsterData)
        {
            base.SpawnInit(monsterData);
            SetBaseOffset().Forget();
        }

        private async UniTaskVoid SetBaseOffset()
        {
            if (baseOffset == 0) return;
            var linearValue = 0f;
            while (navMeshAgent.baseOffset < baseOffset)
            {
                linearValue += Time.deltaTime;
                var offset = Mathf.Lerp(0, baseOffset, linearValue);
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
    }
}