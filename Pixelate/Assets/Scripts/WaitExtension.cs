using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public static class WaitExtension
{
    public static void Wait(this MonoBehaviour mono, float delay, UnityAction action)
    {
        mono.StartCoroutine(ExecuteAction(delay, action));
    }
    private static IEnumerator ExecuteAction(float delay, UnityAction action)
    {
        var wait = new WaitForSecondsRealtime(delay);
        yield return wait;
        action.Invoke();
    }
}
