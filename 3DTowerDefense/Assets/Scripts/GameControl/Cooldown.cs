using UnityEngine;

namespace GameControl
{
    [System.Serializable]
    public class Cooldown
    {
        [SerializeField] private float cooldownTime;
        private float _nextFireTime;

        public bool IsCoolingDown => Time.time < _nextFireTime;
        public void StartCoolDown() => _nextFireTime = Time.time + cooldownTime;
    }
}