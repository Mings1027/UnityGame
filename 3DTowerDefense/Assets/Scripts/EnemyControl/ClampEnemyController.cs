using System;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace EnemyControl
{
    public class ClampEnemyController : MonoBehaviour
    {
        [SerializeField] private Cooldown cooldown;
        [SerializeField] private Transform[] wayPoints;
        [SerializeField] private bool startWave;

        private void Awake()
        {
            wayPoints = new Transform[transform.childCount];
            for (var i = 0; i < wayPoints.Length; i++)
            {
                wayPoints[i] = transform.GetChild(i);
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
            StackObjectPool.Get<ClampEnemy>("Enemy", wayPoints[0].position)
                .OnMoveNexPoint += MoveNextPoint;
            cooldown.StartCoolDown();
        }

        private void MoveNextPoint(ClampEnemy enemy)
        {
            var i = ++enemy.WayPointIndex;
            if (i >= wayPoints.Length)
            {
                enemy.gameObject.SetActive(false);
                return;
            }

            enemy.SetMovePoint(wayPoints[i].position);
        }
    }
}