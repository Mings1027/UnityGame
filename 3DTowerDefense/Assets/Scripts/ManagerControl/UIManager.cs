using System;
using TMPro;
using UnityEngine;

namespace ManagerControl
{
    public sealed class UIManager : MonoBehaviour
    {
        private int _towerCoin;

        public int TowerCoin
        {
            get
            {
                coinText.text = _towerCoin.ToString();
                return _towerCoin;
            }
            set
            {
                _towerCoin = value;
                coinText.text = _towerCoin.ToString();
            }
        }

        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int[] stageStartCoin;

        private void Start()
        {
            TowerCoin = stageStartCoin[0];
        }
    }
}