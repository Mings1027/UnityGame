using System;
using GameControl;
using UnityEngine;

namespace WaveControl
{
    public class WaveManager : MonoBehaviour
    {
        public static Transform[] WayPoints { get; private set; }

        [SerializeField] private Cooldown cooldown;

        [SerializeField] private bool startWave;

        public bool gizmo;

        private void Awake()
        {
            gizmo = true;
            WayPoints = new Transform[transform.childCount];
            for (var i = 0; i < WayPoints.Length; i++)
            {
                WayPoints[i] = transform.GetChild(i);
            }
        }

        private void Update()
        {
            if (startWave)
            {
                EnemySpawn();
            }
        }

        private void EnemySpawn()
        {
            if (cooldown.IsCoolingDown) return;
            StackObjectPool.Get("Enemy", WayPoints[0].position);
            cooldown.StartCoolDown();
        }

        private void OnDrawGizmos()
        {
            if (!gizmo) return;
            for (var i = 0; i < WayPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(WayPoints[i].position, WayPoints[i + 1].position);
            }
        }
    }
}