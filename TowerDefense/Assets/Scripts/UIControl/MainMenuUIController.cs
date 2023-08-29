using ManagerControl;
using MapControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        private CameraManager _cameraManager;
        
        [SerializeField] private Button startButton;
        [SerializeField] private int rotateSpeed;

        private void Awake()
        {
            _cameraManager = FindObjectOfType<CameraManager>();

            startButton.onClick.AddListener(() =>
            {
                StartGame();
                WaveManager.Instance.StartGame();
                TowerManager.Instance.GameStart();
            });
        }

        private void OnEnable()
        {
            Time.timeScale = 1;
        }

        private void Update()
        {
            var rotAmount = Time.deltaTime * rotateSpeed;
            _cameraManager.transform.Rotate(Vector3.up, rotAmount);
        }

        private void StartGame()
        {
            SoundManager.Instance.PlayBGM();
            MapController.Instance.GenerateInitMap();
            _cameraManager.enabled = true;

            gameObject.SetActive(false);
        }
    }
}