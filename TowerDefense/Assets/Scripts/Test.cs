using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ManagerControl;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    private CancellationTokenSource cts;
    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }

    private async UniTaskVoid Start()
    {
        await FindAllObjects();
        PrintFPS().Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private async UniTask FindAllObjects()
    {
        await UniTask.Delay(5000);
        var addressableManager = FindAnyObjectByType<AddressableManager>();
        Destroy(addressableManager);
        print($"===============destroy addressableManager===============");
        var curScene = SceneManager.GetActiveScene();
        var rootObj = curScene.GetRootGameObjects();

        for (int i = 0; i < rootObj.Length; i++)
        {
            if (rootObj[i] == null) continue;
            if (rootObj[i].name == "Test") continue;
            if (rootObj[i].name == "FPS") continue;

            // Destroy(rootObj[i]);
            CreateDestroyButton(rootObj[i].name, rootObj[i]);
            print($"destroy =======>  {rootObj[i]}");
            await UniTask.Delay(100);
        }
    }

    private void CreateDestroyButton(string objName, GameObject sceneObj)
    {
        var obj = Instantiate(prefab, transform);
        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(sceneObj);
            Destroy(obj);
        });
        obj.GetComponentInChildren<TMP_Text>().text = objName;
    }

    private async UniTaskVoid PrintFPS()
    {
        var count = 0;
        while (count < 100)
        {
            count++;
            var fps = 1.0f / Time.deltaTime;
            var ms = Time.deltaTime * 1000f;
            print($"{fps} FPS ({ms} ms)");
            await UniTask.Delay(1000, cancellationToken: cts.Token);
        }
    }
}