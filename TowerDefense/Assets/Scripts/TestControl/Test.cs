using System;
using System.Collections.Generic;
using DG.Tweening;
using StatusControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    [SerializeField] private ParticleSystem a, b;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (a.isPlaying)
            {
                a.Clear();
                a.Stop();
            }

            else if (a.isStopped) a.Play();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (b.isPlaying) b.Stop();
            else if (b.isStopped) b.Play();
        }
    }
}