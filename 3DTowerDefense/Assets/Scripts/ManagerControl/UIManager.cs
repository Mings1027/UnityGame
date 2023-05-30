using System;
using TMPro;
using UIControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ManagerControl
{
    public sealed class UIManager : MonoBehaviour
    {
        private int _towerCoin;
        private int _lifeCount;

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

        public int LifeCount
        {
            get
            {
                lifeCountText.text = _lifeCount.ToString();
                return _lifeCount;
            }
            set
            {
                _lifeCount = value;
                lifeCountText.text = _lifeCount.ToString();
                if (_lifeCount > 0) return;
                gamePlayManager.GameOver();
            }
        }

        [SerializeField] private GamePlayManager gamePlayManager;

        [SerializeField] private TextMeshProUGUI lifeCountText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private int[] stageStartCoin;

        private void Awake()
        {
            gamePlayManager = transform.parent.GetComponent<GamePlayManager>();
        }

        public void Init(int index)
        {
            TowerCoin = stageStartCoin[index];
            LifeCount = 20;
        }
    }
}