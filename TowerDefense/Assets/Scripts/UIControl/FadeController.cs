using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIControl
{
    public class FadeController : MonoBehaviour
    {
        private GraphicRaycaster _graphicRaycaster;

        [SerializeField] private Image fadeInImage;
        [SerializeField] private Image fadeOutImage;

        private void Awake()
        {
            _graphicRaycaster = GetComponent<GraphicRaycaster>();
            _graphicRaycaster.enabled = false;
            fadeInImage.DOFade(1, 0);
            fadeOutImage.DOFade(0, 0);
        }

        private void Start()
        {
            FadeInScene().Forget();
        }

        private async UniTaskVoid FadeInScene()
        {
            await fadeInImage.DOFade(0, 1).From(1).SetUpdate(true).SetDelay(1);
        }

        public async UniTaskVoid FadeOutScene(string sceneName)
        {
            Debug.Log("fade out scene");
            _graphicRaycaster.enabled = true;
            await fadeOutImage.DOFade(1, 1).From(0).SetUpdate(true);
            SceneManager.LoadScene(sceneName);
        }
    }
}