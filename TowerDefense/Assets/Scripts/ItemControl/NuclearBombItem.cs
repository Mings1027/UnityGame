using System;
using CustomEnumControl;
using Cysharp.Threading.Tasks;
using ManagerControl;
using PoolObjectControl;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace ItemControl
{
    public class NuclearBombItem : ItemButton
    {
        private Action _onAllKillMonsterEvent;

        [SerializeField] private AudioClip explosionAudio;

        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.NuclearBomb;
        }

        private void Start()
        {
            _onAllKillMonsterEvent += () => FindAnyObjectByType<WaveManager>().AllKill();
        }

        public override void Spawn()
        {
            Explosion().Forget();
        }

        private async UniTaskVoid Explosion()
        {
            var count = WaveManager.curWave;
            cameraManager.ShakeCamera(count);
            for (var i = 0; i < count; i++)
            {
                var ranPos = cameraManager.camPos + Random.insideUnitSphere * 50;
                NavMesh.SamplePosition(ranPos, out var hit, 100, NavMesh.AllAreas);
                PoolObjectManager.Get(PoolObjectKey.NuclearBomb, hit.position);
                SoundManager.Play3DSound(explosionAudio, ranPos);
                await UniTask.Delay(100);
            }

            _onAllKillMonsterEvent?.Invoke();
        }
    }
}