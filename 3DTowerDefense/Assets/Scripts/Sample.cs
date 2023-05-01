using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    void Start()
    {
        Test t = new Test();
        t.AAA();

        Test2 t2 = new Test2();
        t2.AAA();

        Test3 t3 = new Test3();
        t3.AAA();
    }
}
