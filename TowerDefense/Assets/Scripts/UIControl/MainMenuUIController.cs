using System;
using CustomEnumControl;
using ManagerControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        private CameraManager _camArm;
        public event Action OnGenerateInitMapEvent;

        [SerializeField] private Button startButton;
        [SerializeField] private int rotateSpeed;

        private void Awake()
        {
            _camArm = FindObjectOfType<CameraManager>();
            startButton.onClick.AddListener(StartGame);
        }

        private void Start()
        {
            Time.timeScale = 1;
        }

        private void Update()
        {
            var rotAmount = Time.deltaTime * rotateSpeed;
            _camArm.transform.Rotate(Vector3.up, rotAmount);
        }

        private void StartGame()
        {
            TowerManager.Instance.GameStart();
            SoundManager.Instance.PlaySound(SoundEnum.ButtonSound);
            OnGenerateInitMapEvent?.Invoke();

            _camArm.enabled = true;
            Destroy(gameObject);
        }
    }
}