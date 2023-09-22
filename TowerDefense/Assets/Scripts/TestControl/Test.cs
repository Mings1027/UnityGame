using System;
using PoolObjectControl;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private string testString;
    [SerializeField] private string oldString;

    private void Start()
    {
        var replace = testString.Replace(oldString, "-");
        print(replace);
    }
}