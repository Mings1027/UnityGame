using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniTaskTweenControl
{
    public static class UniTaskTween
    {
        public static void MoveTween(this Transform target, Vector3 endValue, float duration) =>
            UniTaskDoMove(target, endValue, duration).Forget();

        public static void MoveTween(this Transform target, Vector3 startValue, Vector3 endValue, float duration) =>
            UniTaskDoMove(target, startValue, endValue, duration).Forget();

        public static void ScaleTween(this Transform target, Vector3 endValue, float duration) =>
            UniTaskDoScale(target, endValue, duration).Forget();

        public static void ScaleTween(this Transform target, Vector3 endValue, float duration, Action callback) =>
            UniTaskDoScale(target, endValue, duration, callback).Forget();

        public static void OrthoSizeTween(this Camera target, float endValue, float duration) =>
            UniTaskDoOrthoSize(target, endValue, duration).Forget();

        public static void OrthoSizeTween(this Camera target, float startValue, float endValue, float duration) =>
            UniTaskDoOrthoSize(target, startValue, endValue, duration).Forget();

        #region Private Method

        private static async UniTaskVoid UniTaskDoMove(this Transform target, Vector3 endValue, float duration)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            var startValue = target.position;
            while (elapsedTime < duration)
            {
                target.position = Vector3.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        private static async UniTaskVoid UniTaskDoMove(this Transform target, Vector3 startValue, Vector3 endValue,
            float duration)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            while (elapsedTime < duration)
            {
                target.position = Vector3.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        private static async UniTaskVoid UniTaskDoScale(this Transform target, Vector3 endValue, float duration)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            var startValue = target.localScale;
            while (elapsedTime < duration)
            {
                target.localScale = Vector3.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        private static async UniTask UniTaskDoScale(this Transform target, Vector3 endValue, float duration,
            Action callback)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            var startValue = target.localScale;
            while (elapsedTime < duration)
            {
                target.localScale = Vector3.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }

            callback?.Invoke();
        }

        private static async UniTaskVoid UniTaskDoOrthoSize(this Camera target, float endValue, float duration)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            var startValue = target.orthographicSize;
            while (elapsedTime < duration)
            {
                target.orthographicSize = Mathf.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        private static async UniTaskVoid UniTaskDoOrthoSize(this Camera target, float startValue, float endValue,
            float duration)
        {
            var elapsedTime = 0f;
            var inverseDuration = 1.0f / duration;
            while (elapsedTime < duration)
            {
                target.orthographicSize = Mathf.Lerp(startValue, endValue, elapsedTime * inverseDuration);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        #endregion
    }
}