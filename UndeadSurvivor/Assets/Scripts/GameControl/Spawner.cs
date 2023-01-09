using EnemyControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] [Range(0, 30)] private int spawnRange;
        [SerializeField] private SpawnData[] spawnData;
        [SerializeField] private string[] itemName;

        private float _timer;
        private int _level;

        public bool drawGizmos;

        private void Update()
        {
            _timer += Time.deltaTime;
            _level = Mathf.Min(Mathf.FloorToInt(GameManager.Instance.GameTime * 0.1f), spawnData.Length - 1);
            if (_timer > spawnData[_level].spawnTime)
            {
                _timer = 0;
                EnemyRandomSpawn();

                var ranSpawn = Random.Range(0, 3);
                if (ranSpawn == 1) ItemRandomSpawn();
            }
        }

        private void EnemyRandomSpawn()
        {
            var randomPosition = Random.Range(1, 360);
            var rad = Mathf.Deg2Rad * randomPosition;
            // var x = spawnRange * Mathf.Sin(rad);
            // var y = spawnRange * Mathf.Cos(rad);
            spawnPoint.position = GameManager.Instance.player.transform.position +
                                  spawnRange * new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));

            StackObjectPool.Get<EnemyController>("Enemy", spawnPoint.position).Init(spawnData[_level]);
        }

        private void ItemRandomSpawn()
        {
            var randomPosition = Random.Range(1, 360);
            var rad = Mathf.Deg2Rad * randomPosition;
            spawnPoint.position = GameManager.Instance.player.transform.position +
                                  spawnRange * new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
            var ranIndex = Random.Range(0, itemName.Length);
            StackObjectPool.Get(itemName[ranIndex], spawnPoint.position);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Gizmos.DrawWireSphere(GameManager.Instance.player.transform.position, spawnRange);
        }
    }

    [System.Serializable]
    public class SpawnData
    {
        public float spawnTime;
        public int spriteType;
        public int health;
        public float speed;
        public int damage;
    }
}