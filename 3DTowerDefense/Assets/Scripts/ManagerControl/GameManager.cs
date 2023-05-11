using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerControl
{
    public class GameManager : Singleton<GameManager>
    {
        public bool IsPause { get; private set; }

        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button pauseButton;

        private void Awake()
        {
            DOTween.SetTweensCapacity(500, 313);
            pauseButton.onClick.AddListener(Pause);
        }

        private void Pause()
        {
            IsPause = !IsPause;
            Time.timeScale = IsPause ? 0 : 1;
            menuPanel.SetActive(IsPause);
        }
    }
}