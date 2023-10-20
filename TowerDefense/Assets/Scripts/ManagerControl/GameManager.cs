using System;
using Cysharp.Threading.Tasks;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        public TowerManager towerManager { get; private set; }
        public UIManager uiManager { get; private set; }
        public InputManager inputManager { get; private set; }
        public CameraManager cameraManager { get; private set; }
        public WaveManager waveManager { get; private set; }
        public SoundManager soundManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            // Application.targetFrameRate = 60;
            GameInit().Forget();
        }

        private async UniTaskVoid GameInit()
        {
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }

            towerManager = FindObjectOfType<TowerManager>();
            uiManager = FindObjectOfType<UIManager>();
            inputManager = FindObjectOfType<InputManager>();
            cameraManager = FindObjectOfType<CameraManager>();
            waveManager = FindObjectOfType<WaveManager>();
            soundManager = FindObjectOfType<SoundManager>();

            await UniTask.Yield();

            Instantiate(Resources.Load<GameObject>("PoolObjectManager"));
        }
    }
}