using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }
        }
    }


    [System.Serializable]
    public struct Cooldown
    {
        public float cooldownTime { get; set; }
        private float _nextFireTime;

        public bool IsCoolingDown => Time.time < _nextFireTime;
        public void StartCooldown() => _nextFireTime = Time.time + cooldownTime;
    }
}