using System;
using TMPro;
using UnityEngine;

namespace ManagerControl
{
    public sealed class UIManager : MonoBehaviour
    {
        private int towerCoin;

        public int TowerCoin
        {
            get
            {
                coinText.text = towerCoin.ToString();
                return towerCoin;
            }
            set
            {
                towerCoin = value;
                coinText.text = towerCoin.ToString();
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