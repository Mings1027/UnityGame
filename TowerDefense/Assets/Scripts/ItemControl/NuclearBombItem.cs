using CustomEnumControl;
using ManagerControl;
using UIControl;

namespace ItemControl
{
    public class NuclearBombItem : ItemButton
    {
        private WaveManager _waveManager;

        protected override void Start()
        {
            base.Start();
            _waveManager = FindAnyObjectByType<WaveManager>();
        }

        public override bool Spawn()
        {
            if (!_waveManager.isStartWave)
            {
                FloatingNotification.FloatingNotify(FloatingNotifyEnum.OnlyWaveStart);
                return false;
            }

            cameraManager.ShakeCamera();
            _waveManager.AllKill().Forget();
            return true;
        }
    }
}