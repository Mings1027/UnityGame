using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class InformationUIController : MonoBehaviour
    {
        private bool _isPause;

        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button pauseButton;

        private void Awake()
        {
            pauseButton.onClick.AddListener(Pause);
        }

        private void Pause()
        {
            _isPause = !_isPause;
            Time.timeScale = _isPause ? 0 : 1;
            menuPanel.SetActive(_isPause);
        }
    }
}