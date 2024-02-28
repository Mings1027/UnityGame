using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControl
{
    public class FadeController : MonoSingleton<FadeController>
    {
        private GraphicRaycaster _graphicRaycaster;

        [SerializeField] private Image fadeInImage;
        [SerializeField] private Image fadeOutImage;

        protected override void Awake()
        {
            instance = this;
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            fadeInImage.DOFade(1, 0);
            fadeOutImage.DOFade(0, 0);
        }

        private void Start()
        {
            FadeInScene().Forget();
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private async UniTaskVoid FadeInScene()
        {
            _graphicRaycaster.enabled = true;
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            fadeInImage.DOFade(0, 1).From(1).SetUpdate(true);
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            _graphicRaycaster.enabled = false;
        }

        private async UniTaskVoid FadeOutSceneAsync(string sceneName)
        {
            _graphicRaycaster.enabled = true;
            await fadeOutImage.DOFade(1, 1).From(0).SetUpdate(true);
            SceneManager.LoadScene(sceneName);
        }

        public static void FadeOutAndLoadScene(string sceneName)
        {
            instance.FadeOutSceneAsync(sceneName).Forget();
        }
    }
}