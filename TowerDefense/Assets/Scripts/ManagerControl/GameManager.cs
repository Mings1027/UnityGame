using UnityEngine;

namespace ManagerControl
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 30;
            var sources = Resources.LoadAll<GameObject>("Prefabs");
            for (var i = 0; i < sources.Length; i++)
            {
                Instantiate(sources[i]);
            }

        }
    }

    public enum TowerType
    {
        Ballista,
        Assassin,
        Canon,
        Mage,
        Defender
    }
}