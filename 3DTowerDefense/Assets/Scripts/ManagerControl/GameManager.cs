using UIControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject gamePlayPrefab;
  
        private void Awake()
        {
            Instantiate(gamePlayPrefab, transform);
        }
    }
}