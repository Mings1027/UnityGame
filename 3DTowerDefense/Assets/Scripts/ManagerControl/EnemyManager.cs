using EnemyControl;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class EnemyManager : MonoBehaviour
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
            StackObjectPool.Get<Enemy>("Enemy", wayPoints[0].position)
                .OnMoveNexPoint += MoveNextPoint;
            cooldown.StartCoolDown();
        }

        private void MoveNextPoint(Enemy enemy)
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