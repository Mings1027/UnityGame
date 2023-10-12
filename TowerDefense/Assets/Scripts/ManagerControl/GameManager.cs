using System;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        public TowerManager towerManager { get; private set; }
        public InputManager inputManager { get; private set; }
        public CameraManager cameraManager { get; private set; }
        public WaveManager waveManager { get; private set; }
        public SoundManager soundManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            GameInit().Forget();
        }

        private async UniTaskVoid GameInit()
        {
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }

            await UniTask.Yield();
            towerManager = FindObjectOfType<TowerManager>();
            inputManager = FindObjectOfType<InputManager>();
            cameraManager = FindObjectOfType<CameraManager>();
            waveManager = FindObjectOfType<WaveManager>();
            soundManager = FindObjectOfType<SoundManager>();
            Instantiate(Resources.Load<GameObject>("PoolObjManager/7.PoolObjectManager"));
        }
    }

    [Serializable]
    public struct Cooldown
    {
        public float cooldownTime { get; set; }
        private float _nextFireTime;

        public bool IsCoolingDown => Time.time < _nextFireTime;
        public void StartCooldown() => _nextFireTime = Time.time + cooldownTime;
    }
}