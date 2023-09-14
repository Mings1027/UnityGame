using System;
using DataControl;
using ManagerControl;
using MapControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        private CameraManager _cameraManager;

        public event Action OnGenerateInitMapEvent;
        
        [SerializeField] private Button startButton;
        [SerializeField] private int rotateSpeed;

        private void Awake()
        {
            _cameraManager = FindObjectOfType<CameraManager>();

            startButton.onClick.AddListener(StartGame);
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
            SoundManager.Instance.PlayBGM(StringManager.WaveBreak);
            TowerManager.Instance.GameStart();
            
            OnGenerateInitMapEvent?.Invoke();
            
            _cameraManager.enabled = true;
            Destroy(gameObject);
        }
    }
}