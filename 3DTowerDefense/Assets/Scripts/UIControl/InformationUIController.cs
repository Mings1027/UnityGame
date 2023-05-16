using GameControl;
using UnityEngine;

namespace UIControl
{
    public class InformationUIController : Singleton<InformationUIController>
    {
        public int[] TowerCoin => towerCoin;
        public int[] GetTowerCoin => getTowerCoin;
        [SerializeField] private int[] towerCoin;
        [SerializeField] private int[] getTowerCoin;
    }
}
