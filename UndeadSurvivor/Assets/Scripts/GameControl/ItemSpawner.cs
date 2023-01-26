using PlayerControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] [Range(0, 30)] private int spawnRange;
        [SerializeField] private int minRandomTime, maxRandomTime;

        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;
            var ranTime = Random.Range(minRandomTime, maxRandomTime);
            if (_timer < ranTime) return;
            _timer = 0;
            ItemRandomSpawn();
        }

        private void ItemRandomSpawn()
        {
            var randomPosition = Random.Range(1, 360);
            var rad = Mathf.Deg2Rad * randomPosition;
            spawnPoint.position = PlayerController.Rigid.transform.position +
                                  spawnRange * new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
            StackObjectPool.Get("Item", spawnPoint.position);
        }
    }
}