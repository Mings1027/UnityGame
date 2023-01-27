using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float size;

    private void Start()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                print($"x : {x} y : {y} perlin : {Mathf.PerlinNoise(x + size, y + size)}");
            }
        }
    }
}