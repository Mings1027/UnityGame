using UnityEngine;

namespace UIControl
{
    public class TowerButton : MonoBehaviour
    {
        private enum TowerType
        {
            ArcherTower,
            BarracksTower,
            CanonTower,
            MageTower
        }

        public string TowerTypeName => towerType.ToString();

        [SerializeField] private TowerType towerType;
    }
}