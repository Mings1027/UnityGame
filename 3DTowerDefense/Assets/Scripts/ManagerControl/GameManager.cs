using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPrefab;
  
        private void OnEnable()
        {
            Instantiate(gamePlayPrefab, transform);
        }
    }
}