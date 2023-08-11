using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ManagerControl;
using MapControl;
using UnityEngine;
using UnityEngine.UI;

namespace UIControl
{
    public class MainMenuUIController : MonoBehaviour
    {
        private Tween camRotateTween;
        private Camera cam;
        [SerializeField] private Button startButton;

        private void Awake()
        {
            cam = Camera.main;
            startButton.onClick.AddListener(() =>
            {
                StartGame();
                TowerManager.Instance.GameStart();
            });
        }

        private void Start()
        {
            camRotateTween = cam.transform.parent.DORotate(new Vector3(0, 360, 0), 10f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart).SetRelative().SetEase(Ease.Linear);
        }

        private void OnDisable()
        {
            camRotateTween.Kill();
            cam.transform.parent.rotation = Quaternion.Euler(0, 45, 0);
        }

        private void StartGame()
        {
            SoundManager.Instance.PlayBGM();
            MapController.Instance.GenerateInitMap();
            gameObject.SetActive(false);
            cam.orthographic = true;
        }
    }
}