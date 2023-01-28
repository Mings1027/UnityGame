using UnityEngine;
using UnityEngine.Serialization;

namespace TowerControl
{
    public class TowerType : MonoBehaviour
    {
        [SerializeField] private string towerType;

        public string Type => towerType;
    }
}