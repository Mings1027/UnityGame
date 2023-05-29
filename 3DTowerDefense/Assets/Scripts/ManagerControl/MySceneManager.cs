// using System;
// using Cysharp.Threading.Tasks;
// using DG.Tweening;
// using GameControl;
// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// namespace ManagerControl
// {
//     public class MySceneManager : Singleton<MySceneManager>
//     {
//         private int _stageNum;
//         private float _fadeDuration;
//         [SerializeField] private GamePlayManager gamePlayManager;
//         [SerializeField] private CanvasGroup fadeImage;
//
//         private void Start()
//         {
//             _fadeDuration = 0.5f;
//             DontDestroyOnLoad(gameObject);
//
//             SceneManager.sceneLoaded += SceneLoaded;
//         }
//
//         private void OnDestroy()
//         {
//             SceneManager.sceneLoaded -= SceneLoaded;
//         }
//
//         public void ChangeScene(string sceneName)
//         {
//             fadeImage.DOFade(1, _fadeDuration)
//                 .OnStart(() => fadeImage.blocksRaycasts = true)
//                 .OnComplete(() => LoadScene(sceneName).Forget());
//         }
//
//         public void SetStageNumber(int stageNum)
//         {
//             gamePlayManager.SetStage(stageNum);
//         }
//
//         private static async UniTaskVoid LoadScene(string sceneName)
//         {
//             var async = SceneManager.LoadSceneAsync(sceneName);
//             async.allowSceneActivation = false;
//
//             float pastTime = 0;
//             float percentage = 0;
//             while (!async.isDone)
//             {
//                 await UniTask.Yield();
//                 pastTime += Time.deltaTime * 10;
//
//                 if (percentage >= 90)
//                 {
//                     percentage = Mathf.Lerp(percentage, 100, pastTime);
//                     if (Math.Abs(percentage - 100) < float.Epsilon)
//                     {
//                         async.allowSceneActivation = true;
//                     }
//                 }
//                 else
//                 {
//                     percentage = Mathf.Lerp(percentage, async.progress * 100f, pastTime);
//                     if (percentage >= 90) pastTime = 0;
//                 }
//             }
//         }
//
//         private void SceneLoaded(Scene scene, LoadSceneMode mode)
//         {
//             fadeImage.DOFade(0, _fadeDuration)
//                 .OnComplete(() =>
//                 {
//                     fadeImage.blocksRaycasts = false;
//                     gamePlayManager.MapInit();
//                 });
//         }
//     }
// }