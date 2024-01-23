using CustomEnumControl;
using Cysharp.Threading.Tasks;
using PoolObjectControl;
using UIControl;
using UnityEngine;
using UnityEngine.AI;

namespace ItemControl
{
    public class NuclearBombItem : ItemButton
    {
        public override void Spawn()
        {
            Explosion().Forget();
        }

        private async UniTaskVoid Explosion()
        {
            for (var i = 0; i < 10; i++)
            {
                NavMesh.SamplePosition(Vector3.zero, out var hit, 100, NavMesh.AllAreas);
                //hit 된 곳으로부터 y 만큼 떨어진 곳에 폭탄 생성
                //폭탄은 y =0 까지 아래로 낙하 hit에 도착하면 터트림
                PoolObjectManager.Get(PoolObjectKey.CanonHitParticle, hit.position);
                await UniTask.Delay(500);
            }
        }
    }
}