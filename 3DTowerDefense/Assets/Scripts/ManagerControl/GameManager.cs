using UIControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPrefab;
        [SerializeField] private TowerManager towerManager;

        private void Awake()
        {
            Instantiate(gamePlayPrefab, transform);
            Instantiate(towerManager);
        }
    }
}