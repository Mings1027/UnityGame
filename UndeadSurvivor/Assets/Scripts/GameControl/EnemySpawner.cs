using System;
using Cysharp.Threading.Tasks;
using EnemyControl;
using PlayerControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] [Range(0, 30)] private int spawnRange;
        [SerializeField] private SpawnData[] spawnData;

        private float _timer;
        private int _level;

        public bool drawGizmos;

        private void Start()
        {
            InvokeRepeating(nameof(PlusLevel), 1f, 1f);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer < spawnData[_level].spawnTime) return;
            _timer = 0;
            EnemyRandomSpawn();
        }

        private void PlusLevel()
        {
            if (_level >= spawnData.Length - 1) CancelInvoke(nameof(PlusLevel));
            else _level++;
            print(_level);
        }

        private void EnemyRandomSpawn()
        {
            var randomPosition = Random.Range(1, 360);
            var rad = Mathf.Deg2Rad * randomPosition;
            var x = spawnRange * Mathf.Sin(rad);
            var y = spawnRange * Mathf.Cos(rad);
            spawnPoint.position = PlayerController.Rigid.transform.position + new Vector3(x, y);

            StackObjectPool.Get<EnemyAI>("Enemy", spawnPoint.position).Init(spawnData[_level]);
        }
        
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Gizmos.DrawWireSphere(PlayerController.Rigid.transform.position, spawnRange);
        }
    }

    [Serializable]
    public class SpawnData
    {
        public float spawnTime;
        public int spriteType;
        public int health;
        public float speed;
        public int damage;
    }
}