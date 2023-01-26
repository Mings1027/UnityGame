using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    public AIPath aiPath;

    private void Update()
    {
        if (aiPath.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(.5f, .5f, .5f);
        else if (aiPath.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-.5f, .5f, .5f);
    }
}