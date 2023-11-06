using UnityEngine;

namespace DataControl
{
    [CreateAssetMenu]
    public class UnitTowerData : TowerData
    {
        public int UnitHealth => unitHealth;
        [SerializeField] private int unitHealth;
    }
}