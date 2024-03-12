using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIControl
{
    public class FadeController : MonoSingleton<FadeController>
    {
        private GraphicRaycaster _graphicRaycaster;

        [SerializeField] private CanvasGroup fadeInGroup;
        [SerializeField] private Image fadeOutImage;
        [SerializeField] private Slider loadingSlider;

        protected override void Awake()
        {
            base.Awake();
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            fadeInGroup.DOFade(1, 0);
            fadeOutImage.DOFade(0, 0);
        }

        private void Start()
        {
            FadeInScene();
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private void FadeInScene()
        {
            _graphicRaycaster.enabled = true;
            loadingSlider.DOValue(1, 1).From(0).SetDelay(1).SetUpdate(true).OnComplete(() =>
            {
                fadeInGroup.DOFade(0, 0.5f).From(1).SetUpdate(true);
                _graphicRaycaster.enabled = false;
            });
        }

        private async UniTaskVoid FadeOutSceneAsync(string sceneName)
        {
            _graphicRaycaster.enabled = true;
            await fadeOutImage.DOFade(1, 0.5f).From(0).SetUpdate(true);
            SceneManager.LoadScene(sceneName);
        }

        public static void FadeOutAndLoadScene(string sceneName)
        {
            instance.FadeOutSceneAsync(sceneName).Forget();
        }
    }
}