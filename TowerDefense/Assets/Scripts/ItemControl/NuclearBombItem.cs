using ManagerControl;

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

        public override void Spawn()
        {
            Explosion();
        }

        private void Explosion()
        {
            cameraManager.ShakeCamera();
            _waveManager.AllKill().Forget();
        }
    }
}