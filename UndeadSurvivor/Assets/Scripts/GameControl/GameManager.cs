using PlayerControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameControl
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public float GameTime { get; private set; }
        public PlayerController player;

        [SerializeField] private float maxGameTime = 6 * 10;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            GameTime += Time.deltaTime;
            if (GameTime > maxGameTime)
            {
                GameTime = maxGameTime;
            }
        }
    }
}