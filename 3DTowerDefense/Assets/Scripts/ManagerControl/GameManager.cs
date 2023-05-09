using DG.Tweening;
using GameControl;
using UnityEngine;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        public bool IsPause { get; private set; }

        [SerializeField] private GameObject menuPanel;

        private void Awake()
        {
            DOTween.SetTweensCapacity(500, 313);
        }

        private void Start()
        {
            
        }

        private void Pause()
        {
            IsPause = true;
            Time.timeScale = 0;
            menuPanel.SetActive(true);
        }

        private void Resume()
        {
            IsPause = false;
            Time.timeScale = 1;
            menuPanel.SetActive(false);
        }
    }
}