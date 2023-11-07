using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ManagerControl
{
    public class LoadingManager : MonoBehaviour
    {
        public static string nextScene;

        [SerializeField] private Slider loadingBar;
    
        private void Start()
        {
            StartLoadingScene().Forget();
        }

        private async UniTaskVoid StartLoadingScene()
        {
            var op = SceneManager.LoadSceneAsync(nextScene);
            op.allowSceneActivation = false;

            var timer = 0f;

            while (!op.isDone)
            {
                await UniTask.Yield();

                timer += Time.deltaTime;

                if (op.progress < 0.9f)
                {
                    loadingBar.value = Mathf.Lerp(loadingBar.value, op.progress, timer);
                    if (loadingBar.value >= op.progress)
                    {
                        timer = 0;
                    }
                }
                else
                {
                    loadingBar.value = Mathf.Lerp(loadingBar.value, 1f, timer);
                    if (loadingBar.value == 1f)
                    {
                        await UniTask.Delay(1000);
                        op.allowSceneActivation = true;
                        break;
                    }
                }
            }
        }

        public static void LoadScene(string sceneName)
        {
            nextScene = sceneName;
            SceneManager.LoadScene("Loading");
        }
    }
}
