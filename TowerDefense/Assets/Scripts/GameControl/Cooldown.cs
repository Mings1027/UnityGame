using System;
using UnityEngine;

namespace GameControl
{
    [Serializable]
    public struct Cooldown
    {
        private float _nextFireTime;

        public float cooldownTime { get; set; }
        public bool IsCoolingDown => Time.time < _nextFireTime;
        public void StartCooldown() => _nextFireTime = Time.time + cooldownTime;
    }
}