using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncTest : MonoBehaviour
{
    private CancellationTokenSource disableCancellation;
    private CancellationTokenSource aaaCancellation;

    private void OnEnable()
    {
        disableCancellation?.Dispose();

        disableCancellation = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        disableCancellation.Cancel();
    }

    public void StartAaa()
    {
        Aaa().Forget();
    }

    public void StopAaa()
    {
        aaaCancellation.Cancel();
        aaaCancellation.Dispose();
    }

    public void StartBbb()
    {
        Bbb().Forget();
    }

    private async UniTask Aaa()
    {
        var i = 0;
        aaaCancellation = new CancellationTokenSource();
        var link = CancellationTokenSource.CreateLinkedTokenSource(disableCancellation.Token, aaaCancellation.Token);
        while (i < 20)
        {
            Debug.Log("AAA : " + i++);
            await UniTask.Delay(1000, cancellationToken: link.Token);
        }

        Debug.Log("AAA 완료 !");
    }

    private async UniTask Bbb()
    {
        var i = 100;
        while (i < 120)
        {
            Debug.Log("BBB : " + i++);
            await UniTask.Delay(1000, cancellationToken: disableCancellation.Token);
        }

        Debug.Log("BBB 완료 !");
    }
}