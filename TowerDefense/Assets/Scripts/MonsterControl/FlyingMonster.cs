using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MonsterControl
{
    public class FlyingMonster : MonsterUnit
    {
        [SerializeField] private byte baseOffset;

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
    }
}