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
        private Transform _camArm;
        public event Action OnGenerateInitMapEvent;

        [SerializeField] private Button startButton;
        [SerializeField] private int rotateSpeed;

        private void Awake()
        {
            _camArm = CameraManager.Instance.transform;
            startButton.onClick.AddListener(StartGame);
        }

        private void OnEnable()
        {
            Time.timeScale = 1;
        }

        private void Update()
        {
            var rotAmount = Time.deltaTime * rotateSpeed;
            _camArm.Rotate(Vector3.up, rotAmount);
        }

        private void StartGame()
        {
            // SoundManager.Instance.PlayBGM(StringManager.WaveBreak);
            TowerManager.Instance.GameStart();

            OnGenerateInitMapEvent?.Invoke();

            CameraManager.Instance.enabled = true;
            Destroy(gameObject);
        }
    }
}